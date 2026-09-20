using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Opera;
using Render;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-200)]
public class OperaGameController : MonoBehaviour
{
    public static OperaGameController Instance { get; private set; }
    public bool Ready { get; private set; }
    public bool Thinking { get; private set; }
    public string Status { get; private set; } = "Starting Opera…";
    public bool CanHumanMove => Ready && !Thinking && !Reviewing && pendingPromotion == null && GameManager.Instance.IsMyTurn();
    public GameState State => record.Live;
    public bool Reviewing => viewedPly != record.Plies.Count;
    public int ViewedPly => viewedPly;
    public OperaGameRecord Record => record;
    public bool ShowAnalysis { get; private set; } = true;
    private OperaGameRecord record;
    private UciEngineClient engine, analysisEngine;
    private CancellationTokenSource session, analysisSession;
    private readonly ConcurrentQueue<Action> uiUpdates = new ConcurrentQueue<Action>();
    private int thinkMilliseconds = 1000, viewedPly, analysisGeneration;
    private Move pendingPromotion;
    private OperaPlayPanel panel;
    private UciSearchInfo lastAnalysis;
    private Square[] renderedSquares;
    private string exportsDirectory;

    [Serializable] private class EngineMetadata { public string packaged_from_checkout; public string sha256; }
    private void Awake()
    {
        Instance = this;
        if (GameManager.Instance == null) new GameObject("Game Manager").AddComponent<GameManager>();
        GameManager.Instance.StartEngineGame(PieceColor.White);
        CreateRecord(PieceColor.White);
    }
    private async void Start()
    {
        panel = new OperaPlayPanel(transform, this);
        await Task.Yield();
        if (this == null) return;
        DrawPosition();
        await StartEngineAsync();
    }
    private void Update()
    {
        while (uiUpdates.TryDequeue(out Action update)) update();
        panel?.Resize();
        var keyboard = Keyboard.current;
        if (keyboard == null || pendingPromotion != null || panel == null) return;
        if (keyboard.leftArrowKey.wasPressedThisFrame) ViewPly(viewedPly - 1);
        if (keyboard.rightArrowKey.wasPressedThisFrame) ViewPly(viewedPly + 1);
        if (keyboard.homeKey.wasPressedThisFrame) ViewPly(0);
        if (keyboard.endKey.wasPressedThisFrame) ViewPly(record.Plies.Count);
    }
    private void CreateRecord(PieceColor color)
    {
        record = new OperaGameRecord(color) { MoveMilliseconds = thinkMilliseconds };
        GameManager.Instance.GameState = record.Live;
        viewedPly = 0;
        try
        {
            string path = Path.Combine(Path.GetDirectoryName(OperaEngineLocator.FindExecutable()), "engine.json");
            if (File.Exists(path))
            {
                var metadata = JsonUtility.FromJson<EngineMetadata>(File.ReadAllText(path));
                record.EngineRevision = metadata.packaged_from_checkout;
                record.EngineHash = metadata.sha256;
            }
        }
        catch (Exception error) { Debug.LogWarning("[Opera] Engine version metadata: " + error.Message); }
    }
    public async Task NewGameAsync(PieceColor color)
    {
        if (!ArchiveBeforeReplacing()) return;
        StopAll();
        BoardInputManager.Instance?.UnselectSquare();
        GameManager.Instance.StartEngineGame(color);
        CreateRecord(color);
        pendingPromotion = null; panel.ShowPromotion(false);
        DrawPosition();
        await StartEngineAsync();
    }
    public async Task ResumeHereAsync()
    {
        if (!Reviewing || !ArchiveBeforeReplacing()) return;
        StopAll();
        BoardInputManager.Instance?.UnselectSquare();
        record.ResumeAt(viewedPly);
        GameManager.Instance.GameState = record.Live;
        pendingPromotion = null; panel.ShowPromotion(false);
        DrawPosition();
        panel.Notice("Original game saved. Continuing from this position.");
        await StartEngineAsync();
    }
    private async Task StartEngineAsync()
    {
        Ready = false; SetStatus("Starting Opera…");
        session = new CancellationTokenSource();
        var cancellation = session.Token;
        var client = new UciEngineClient(); engine = client;
        try
        {
            await client.StartAsync(OperaEngineLocator.FindExecutable(), cancellation);
            if (cancellation.IsCancellationRequested || engine != client) return;
            record.EngineName = client.EngineName;
            Ready = true; RefreshStatus(); RestartAnalysis();
            await MoveForEngineAsync();
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { if (!cancellation.IsCancellationRequested && engine == client) ReportError(error); }
    }
    public bool TryHumanMove(Move move)
    {
        if (!CanHumanMove || move.From.Piece.GetColor() != GameManager.Instance.MyColor || !LegalMovesHandler.IsMoveLegal(State, move.To, move.From)) return false;
        if (move.From.Piece.GetType() == PieceType.Pawn && (move.To.Coord.rank == 0 || move.To.Coord.rank == 7))
        {
            pendingPromotion = move; panel.ShowPromotion(true); SetStatus("Choose a promotion piece."); return true;
        }
        return ApplyHumanMove(move);
    }
    private bool ApplyHumanMove(Move move)
    {
        if (!record.TryMove(move.ToUci())) return false;
        viewedPly = record.Plies.Count;
        DrawPosition(); RefreshStatus(); RestartAnalysis();
        _ = MoveForEngineAsync();
        return true;
    }
    public void Promote(PieceType type)
    {
        if (pendingPromotion == null) return;
        Move move = pendingPromotion; pendingPromotion = null; panel.ShowPromotion(false);
        move.Promotion = type; ApplyHumanMove(move);
    }
    private async Task MoveForEngineAsync()
    {
        if (!Ready || State.IsGameOver || GameManager.Instance.IsMyTurn() || Thinking) return;
        var client = engine; var cancellation = session.Token; var game = record;
        int ply = game.Plies.Count; var turn = State.ColorToMove;
        Thinking = true; RefreshStatus();
        try
        {
            string response = await client.GetMoveAsync(game.History(ply), thinkMilliseconds, cancellation, info =>
                uiUpdates.Enqueue(() => {
                    if (record != game || engine != client || cancellation.IsCancellationRequested) return;
                    game.Annotate(ply, info, turn);
                    if (ShowAnalysis && viewedPly == ply && lastAnalysis == null)
                        panel.Analysis(new[] { info }, game.PositionAt(ply), "Opponent's search");
                }));
            if (cancellation.IsCancellationRequested || engine != client || record != game) return;
            bool follow = !Reviewing;
            if (response == null || !game.TryMove(response)) throw new InvalidOperationException("Opera returned an invalid move: " + response);
            Thinking = false;
            if (follow) { viewedPly = record.Plies.Count; DrawPosition(); RestartAnalysis(); }
            else panel.History(record, viewedPly);
            GameManager.Instance.NotifyStateUpdated(); RefreshStatus();
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { if (!cancellation.IsCancellationRequested && engine == client) ReportError(error); }
    }
    public void ViewPly(int ply)
    {
        if (pendingPromotion != null) return;
        ply = Math.Max(0, Math.Min(record.Plies.Count, ply));
        if (ply == viewedPly) return;
        BoardInputManager.Instance?.UnselectSquare();
        viewedPly = ply; DrawPosition(); RefreshStatus(); RestartAnalysis();
    }
    private void DrawPosition()
    {
        if (panel == null || BoardRenderer.Instance == null) return;
        var shown = Reviewing ? record.PositionAt(viewedPly) : State;
        if (renderedSquares != shown.Board.squares)
        {
            BoardRenderer.Instance.RenderBoardSquares(shown.Board);
            if (record.HumanColor == PieceColor.Black) BoardRenderer.Instance.FlipPerspective(shown.Board);
            renderedSquares = shown.Board.squares;
        }
        else foreach (Square square in shown.Board.squares)
        {
            if (square.Renderer != null) square.Renderer.RemoveHighlight();
            BoardRenderer.Instance.RenderPieceOnBoard(square);
        }
        if (viewedPly > 0)
        {
            string uci = record.Plies[viewedPly - 1].Uci;
            shown.Board.GetSquareFromNotation(uci.Substring(0, 2)).Renderer?.AddHighlight();
            shown.Board.GetSquareFromNotation(uci.Substring(2, 2)).Renderer?.AddHighlight();
        }
        panel.History(record, viewedPly);
        GameManager.Instance.NotifyStateUpdated();
    }
    public void ToggleAnalysis()
    {
        ShowAnalysis = !ShowAnalysis;
        panel.AnalysisVisible(ShowAnalysis);
        RestartAnalysis();
    }
    private void RestartAnalysis()
    {
        StopAnalysis(); lastAnalysis = null;
        if (!ShowAnalysis || panel == null) return;
        var position = record.PositionAt(viewedPly);
        if (position.IsGameOver)
        {
            panel.TerminalAnalysis(position); return;
        }
        panel.AnalysisPending();
        analysisSession = new CancellationTokenSource();
        _ = AnalyzeAsync(record, viewedPly, analysisGeneration, position, analysisSession.Token);
    }
    private async Task AnalyzeAsync(OperaGameRecord game, int ply, int generation, GameState position, CancellationToken cancellation)
    {
        UciEngineClient client = null;
        try
        {
            // Debounce quick history navigation before starting a child process.
            await Task.Delay(100, cancellation);
            cancellation.ThrowIfCancellationRequested();
            if (generation != analysisGeneration || game != record) return;
            client = new UciEngineClient(); analysisEngine = client;
            await client.StartAsync(OperaEngineLocator.FindExecutable(), cancellation);
            string[] legal = ChessNotation.LegalUciMoves(position);
            foreach (int milliseconds in new[] { 150, 500, 1500 })
            {
                var remaining = new List<string>(legal);
                var lines = new List<UciSearchInfo>();
                for (int candidate = 0; candidate < 3 && remaining.Count > 0; candidate++)
                {
                    int index = candidate; UciSearchInfo latest = null;
                    string move = await client.GetMoveAsync(game.History(ply), milliseconds, cancellation, info => {
                        if (info.Pv.Length == 0 || !remaining.Contains(info.Pv[0]) || info.IsBound) return;
                        latest = info;
                        var snapshot = lines.Concat(new[] { info }).ToArray();
                        uiUpdates.Enqueue(() => {
                            if (cancellation.IsCancellationRequested || generation != analysisGeneration || game != record || ply != viewedPly) return;
                            if (index == 0) { lastAnalysis = info; game.Annotate(ply, info, position.ColorToMove); }
                            panel.Analysis(snapshot, position, "Analyzing · scores favour White when positive", lastAnalysis);
                        });
                    }, candidate == 0 ? null : remaining);
                    if (move == null) break;
                    if (!remaining.Remove(move)) throw new InvalidDataException("Analysis returned a move outside its candidates.");
                    if (latest != null && latest.Pv[0] == move) lines.Add(latest);
                }
                var completed = lines.ToArray();
                uiUpdates.Enqueue(() => {
                    if (!cancellation.IsCancellationRequested && generation == analysisGeneration && game == record && ply == viewedPly)
                        panel.Analysis(completed, position, milliseconds == 1500 ? "Opera's estimate · + White / − Black" : "Refining candidates…", lastAnalysis);
                });
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception error)
        {
            if (!cancellation.IsCancellationRequested && generation == analysisGeneration)
                panel?.AnalysisUnavailable(error.Message);
        }
        finally
        {
            client?.Dispose();
            if (analysisEngine == client) analysisEngine = null;
        }
    }
    private void RefreshStatus()
    {
        if (Reviewing)
        {
            SetStatus("Reviewing move " + ((viewedPly + 1) / 2) + " · " + (viewedPly % 2 == 0 ? "White" : "Black") + " to move" +
                (Thinking ? "\nOpera is thinking at the live position." : "\nReturn to Live or resume from here.")); return;
        }
        if (State.IsGameOver)
        {
            SetStatus(record.Termination == "resignation" ? "You resigned. Opera wins." : State.Result == GameStateUtils.GameResult.Checkmate
                ? State.ColorToMove == record.HumanColor ? "Checkmate. Opera wins." : "Checkmate. You win!"
                : "Draw · " + State.Result);
            Ready = false; engine?.Dispose(); engine = null; return;
        }
        string turn = Thinking ? "Opera is thinking…" : GameManager.Instance.IsMyTurn() ? "Your turn · " + record.HumanColor : "Opera's turn";
        if (CheckUtils.IsKingInCheck(State.Board, State.ColorToMove)) turn += " · Check";
        SetStatus(turn);
    }
    public void SetThinkingTime(int milliseconds)
    {
        thinkMilliseconds = milliseconds; record.MoveMilliseconds = milliseconds; panel.ThinkingTime(milliseconds);
    }
    public void Resign()
    {
        if (State.IsGameOver) return;
        StopAll(); record.Resign(); pendingPromotion = null; panel.ShowPromotion(false);
        viewedPly = record.Plies.Count; DrawPosition(); RefreshStatus(); RestartAnalysis();
    }
    private static string GameDirectory()
    {
        string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        // Unity's Mono maps MyDocuments to the home directory on some Unix hosts.
        if (!string.IsNullOrEmpty(profile) && (string.IsNullOrEmpty(documents) || documents == profile))
            documents = Path.Combine(profile, "Documents");
        return Path.Combine(string.IsNullOrEmpty(documents) ? Application.persistentDataPath : documents, "Opera Chess", "Games");
    }
    private string SaveGame()
    {
        exportsDirectory = GameDirectory();
        Directory.CreateDirectory(exportsDirectory);
        string path = Path.Combine(exportsDirectory, "Opera-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + "-" + Guid.NewGuid().ToString("N").Substring(0, 4) + ".pgn");
        File.WriteAllText(path, record.ExportPgn(viewedPly), new UTF8Encoding(false));
        return path;
    }
    public void ExportGame()
    {
        try
        {
            string path = SaveGame(); GUIUtility.systemCopyBuffer = record.ExportPgn(viewedPly);
            panel.Notice("PGN saved and copied. Use Show exports to find " + Path.GetFileName(path));
        }
        catch (Exception error) { panel.Notice("Export failed: " + error.Message); }
    }
    public void ShowExports()
    {
        try
        {
            exportsDirectory = GameDirectory(); Directory.CreateDirectory(exportsDirectory);
            Application.OpenURL(new Uri(exportsDirectory + Path.DirectorySeparatorChar).AbsoluteUri);
        }
        catch (Exception error) { panel.Notice("Could not open exports: " + error.Message); }
    }
    private bool ArchiveBeforeReplacing()
    {
        if (record.Plies.Count == 0) return true;
        try { SaveGame(); return true; }
        catch (Exception error) { panel.Notice("Could not save the original game: " + error.Message); return false; }
    }
    private void ReportError(Exception error)
    {
        Debug.LogError("[Opera] " + error); StopEngine(); SetStatus("Opera stopped. Start a new game or resume an earlier move.\n" + error.Message);
    }
    private void SetStatus(string value) { Status = value; panel?.Status(value); }
    private void StopAnalysis()
    {
        analysisGeneration++; analysisSession?.Cancel(); analysisEngine?.Dispose(); analysisEngine = null;
        analysisSession?.Dispose(); analysisSession = null;
    }
    private void StopEngine()
    {
        Ready = false; Thinking = false; session?.Cancel(); engine?.Dispose(); engine = null; session?.Dispose(); session = null;
    }
    private void StopAll() { StopEngine(); StopAnalysis(); }
    public void ReturnToMenu()
    {
        if (!ArchiveBeforeReplacing()) return;
        StopAll(); GameManager.Instance.ResetToDefault(); SceneManager.LoadScene(SceneLoader.MainMenu);
    }
    private void OnDestroy() { StopAll(); if (Instance == this) Instance = null; }
    private void OnApplicationQuit()
    {
        if (record != null && record.Plies.Count > 0) { try { SaveGame(); } catch (Exception error) { Debug.LogWarning("[Opera] Autosave: " + error.Message); } }
        StopAll();
    }
}
