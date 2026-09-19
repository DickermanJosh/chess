using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Opera
{
    /// <summary>One owned UCI child process. All pipe reading happens off Unity's main thread.</summary>
    public sealed class UciEngineClient : IDisposable
    {
        private readonly ConcurrentQueue<string> output = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> diagnostics = new ConcurrentQueue<string>();
        private readonly SemaphoreSlim available = new SemaphoreSlim(0);
        private readonly SemaphoreSlim commandGate = new SemaphoreSlim(1, 1);
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private Process process;
        private Exception failure;
        private int disposed;
        public string EngineName { get; private set; } = "Opera";
        public int ProcessId => process?.Id ?? -1;

        public async Task StartAsync(string executable, CancellationToken cancellation)
        {
            if (process != null) throw new InvalidOperationException("This engine client has already started.");
            cancellation.ThrowIfCancellationRequested();
            if (!File.Exists(executable)) throw new FileNotFoundException("Opera executable is missing", executable);
            process = new Process { StartInfo = new ProcessStartInfo {
                FileName = executable,
                WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(executable)),
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
                StandardInputEncoding = new UTF8Encoding(false), StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            }};
            process.Start();
            process.StandardInput.AutoFlush = true;
            _ = Task.Run(ReadOutputAsync);
            _ = Task.Run(ReadDiagnosticsAsync);
            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellation, lifetime.Token))
            {
                Send("uci");
                await UntilAsync("uciok", 5000, linked.Token).ConfigureAwait(false);
                Send("setoption name Hash value 16");
                Send("setoption name Threads value 1");
                Send("setoption name Ponder value false");
                Send("setoption name MorphyStyle value false");
                Send("setoption name Move Overhead value 10");
                Send("ucinewgame");
                Send("isready");
                await UntilAsync("readyok", 5000, linked.Token).ConfigureAwait(false);
            }
        }

        public async Task<string> GetMoveAsync(IReadOnlyList<string> moves, int milliseconds, CancellationToken cancellation)
        {
            if (milliseconds < 1 || milliseconds > 30000) throw new ArgumentOutOfRangeException(nameof(milliseconds));
            var position = new StringBuilder("position startpos");
            if (moves.Count > 0) position.Append(" moves");
            foreach (string move in moves)
            {
                if (!IsCoordinateMove(move)) throw new FormatException("Invalid move in game history: " + move);
                position.Append(' ').Append(move);
            }
            if (position.Length > 4096) throw new InvalidOperationException("Game history exceeds the engine's command limit.");
            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellation, lifetime.Token))
            {
                await commandGate.WaitAsync(linked.Token).ConfigureAwait(false);
                try
                {
                    Send(position.ToString());
                    Send("isready");
                    await UntilAsync("readyok", 5000, linked.Token).ConfigureAwait(false);
                    Send("go movetime " + milliseconds);
                    string response = await UntilAsync("bestmove ", milliseconds + 5000, linked.Token).ConfigureAwait(false);
                    string move = response.Split(' ')[1];
                    if (move == "0000" || move == "(none)") return null;
                    if (!IsCoordinateMove(move)) throw new InvalidDataException("Invalid engine bestmove: " + response);
                    return move;
                }
                catch
                {
                    // A cancelled/failed search must never leave a stale bestmove
                    // queued for a later game. This client cannot be reused.
                    Dispose();
                    throw;
                }
                finally { commandGate.Release(); }
            }
        }

        private static bool IsCoordinateMove(string move) => move != null &&
            (move.Length == 4 || (move.Length == 5 && "qrbn".IndexOf(move[4]) >= 0)) &&
            move[0] >= 'a' && move[0] <= 'h' && move[2] >= 'a' && move[2] <= 'h' &&
            move[1] >= '1' && move[1] <= '8' && move[3] >= '1' && move[3] <= '8';

        private void Send(string command)
        {
            if (Volatile.Read(ref disposed) != 0) throw new ObjectDisposedException(nameof(UciEngineClient));
            process.StandardInput.WriteLine(command);
        }

        private async Task<string> UntilAsync(string prefix, int milliseconds, CancellationToken cancellation)
        {
            var clock = Stopwatch.StartNew();
            while (true)
            {
                cancellation.ThrowIfCancellationRequested();
                var error = Volatile.Read(ref failure);
                if (error != null) throw new IOException("Opera stopped responding. " + string.Join("\n", diagnostics), error);
                int remaining = milliseconds - (int)clock.ElapsedMilliseconds;
                if (remaining <= 0 || !await available.WaitAsync(remaining, cancellation).ConfigureAwait(false))
                    throw new TimeoutException("Opera did not return " + prefix.Trim() + " within its time limit.");
                if (!output.TryDequeue(out string line)) continue;
                if (line.StartsWith("info string ERROR", StringComparison.Ordinal))
                    throw new InvalidDataException(line);
                if (line.StartsWith("id name ", StringComparison.Ordinal)) EngineName = line.Substring(8);
                if (line.StartsWith(prefix, StringComparison.Ordinal)) return line;
            }
        }

        private async Task ReadOutputAsync()
        {
            try
            {
                string line;
                while ((line = await process.StandardOutput.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (output.Count >= 512) throw new InvalidDataException("Opera output queue exceeded its limit.");
                    output.Enqueue(line.Trim());
                    available.Release();
                }
                if (Volatile.Read(ref disposed) == 0) throw new EndOfStreamException("Opera closed stdout.");
            }
            catch (Exception error)
            {
                if (Volatile.Read(ref disposed) == 0) Interlocked.CompareExchange(ref failure, error, null);
            }
            finally { available.Release(); }
        }

        private async Task ReadDiagnosticsAsync()
        {
            try
            {
                string line;
                while ((line = await process.StandardError.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    diagnostics.Enqueue(line.Length > 512 ? line.Substring(0, 512) : line);
                    while (diagnostics.Count > 16) diagnostics.TryDequeue(out _);
                }
            }
            catch (IOException) { }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0) return;
            lifetime.Cancel();
            if (process == null) return;
            try
            {
                if (!process.HasExited)
                {
                    try { process.StandardInput.WriteLine("stop"); process.StandardInput.WriteLine("quit"); }
                    catch (IOException) { }
                    if (!process.WaitForExit(200)) process.Kill();
                }
            }
            catch (InvalidOperationException) { }
            catch (System.ComponentModel.Win32Exception) { }
            finally { process.Dispose(); }
        }
    }
}
