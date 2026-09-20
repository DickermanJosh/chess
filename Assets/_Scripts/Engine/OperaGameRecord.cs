using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Core;

namespace Opera
{
    /// <summary>Immutable move text plus a live rules state; review never mutates the live game.</summary>
    public sealed class OperaGameRecord
    {
        public sealed class Ply
        {
            public string Uci { get; }
            public string San { get; }
            public string Fen { get; }
            public Ply(string uci, string san, string fen) { Uci = uci; San = san; Fen = fen; }
        }
        private readonly List<Ply> plies = new List<Ply>();
        private readonly Dictionary<int, string> evaluations = new Dictionary<int, string>();
        public IReadOnlyList<Ply> Plies => plies;
        public GameState Live { get; private set; } = new GameState();
        public PieceColor HumanColor { get; }
        public DateTime Started { get; } = DateTime.Now;
        public string EngineName { get; set; } = "Opera";
        public string EngineRevision { get; set; } = "unknown";
        public string EngineHash { get; set; } = "unknown";
        public int MoveMilliseconds { get; set; } = 1000;
        private string resignationResult;
        public OperaGameRecord(PieceColor humanColor) { HumanColor = humanColor; }

        public bool TryMove(string uci)
        {
            string san = ChessNotation.ApplySan(Live, uci);
            if (san == null) return false;
            plies.Add(new Ply(uci, san, Live.CurrentFen));
            return true;
        }
        public string[] History(int ply) => plies.Take(ply).Select(p => p.Uci).ToArray();
        public GameState PositionAt(int ply)
        {
            if (ply < 0 || ply > plies.Count) throw new ArgumentOutOfRangeException(nameof(ply));
            var state = new GameState();
            foreach (string uci in History(ply))
                if (!state.TryApplyMove(Move.FromUci(state, uci))) throw new InvalidOperationException("Recorded move failed to replay: " + uci);
            return state;
        }
        public void ResumeAt(int ply)
        {
            // Replaying restores castling, en passant, clocks AND repetition history.
            Live = PositionAt(ply);
            plies.RemoveRange(ply, plies.Count - ply);
            foreach (int key in evaluations.Keys.Where(k => k > ply).ToArray()) evaluations.Remove(key);
            resignationResult = null;
        }
        public void Resign()
        {
            resignationResult = HumanColor == PieceColor.White ? "0-1" : "1-0";
            Live.IsGameOver = true;
        }
        public string Result => resignationResult ?? (Live.Result == GameStateUtils.GameResult.Checkmate
            ? (Live.ColorToMove == PieceColor.White ? "0-1" : "1-0")
            : Live.Result == GameStateUtils.GameResult.InProgress ? "*" : "1/2-1/2");
        public string Termination => resignationResult != null ? "resignation" :
            Live.Result == GameStateUtils.GameResult.InProgress ? "unterminated" : Live.Result.ToString();
        public void Annotate(int ply, UciSearchInfo info, PieceColor turn)
        {
            if (ply < 0 || ply > plies.Count || info == null || !info.HasScore || info.IsBound) return;
            int sign = turn == PieceColor.White ? 1 : -1;
            string score = info.Mate.HasValue ? "#" + (sign * info.Mate.Value).ToString(CultureInfo.InvariantCulture)
                : (sign * info.Centipawns.Value / 100.0).ToString("0.00", CultureInfo.InvariantCulture);
            evaluations[ply] = "[%eval " + score + "," + info.Depth + "]";
        }
        private static string Escape(string text) => (text ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", " ").Replace("\n", " ");
        public string ExportPgn(int viewedPly)
        {
            var output = new StringBuilder();
            void Tag(string name, string value) => output.Append('[').Append(name).Append(" \"").Append(Escape(value)).Append("\"]\n");
            Tag("Event", "Opera Chess playtest"); Tag("Site", "Local"); Tag("Date", Started.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture));
            Tag("Round", "-"); Tag("White", HumanColor == PieceColor.White ? "Human" : EngineName);
            Tag("Black", HumanColor == PieceColor.Black ? "Human" : EngineName); Tag("Result", Result);
            Tag("Termination", Termination); Tag("EngineCommit", EngineRevision); Tag("EngineSHA256", EngineHash);
            Tag("MorphyStyle", "true"); Tag("MoveTimeMilliseconds", MoveMilliseconds.ToString(CultureInfo.InvariantCulture));
            Tag("ViewedPly", viewedPly.ToString(CultureInfo.InvariantCulture));
            Tag("ViewedFEN", viewedPly == 0 ? FENUtils.StartFen : plies[viewedPly - 1].Fen); Tag("FinalFEN", Live.CurrentFen);
            output.Append('\n');
            if (evaluations.TryGetValue(0, out string initial)) output.Append("{ ").Append(initial).Append(" } ");
            for (int i = 0; i < plies.Count; ++i)
            {
                if (i % 2 == 0) output.Append(i / 2 + 1).Append(". ");
                output.Append(plies[i].San).Append(' ');
                if (evaluations.TryGetValue(i + 1, out string annotation)) output.Append("{ ").Append(annotation).Append(" } ");
                if (i % 2 == 1) output.Append('\n');
            }
            return output.Append(Result).Append('\n').ToString();
        }
    }

    public static class ChessNotation
    {
        private static string Letter(PieceType type) => type switch {
            PieceType.Knight => "N", PieceType.Bishop => "B", PieceType.Rook => "R", PieceType.Queen => "Q", PieceType.King => "K", _ => ""
        };
        public static string ApplySan(GameState state, string uci)
        {
            Move move = Move.FromUci(state, uci);
            Square from = move.From, to = move.To;
            PieceType type = from.Piece.GetType();
            if (state.IsGameOver || from.Piece.GetColor() != state.ColorToMove || !LegalMovesHandler.IsMoveLegal(state, to, from)) return null;
            string san;
            if (type == PieceType.King && Math.Abs(to.Coord.file - from.Coord.file) == 2)
                san = to.Coord.file > from.Coord.file ? "O-O" : "O-O-O";
            else
            {
                bool capture = to.Piece.GetType() != PieceType.None || (type == PieceType.Pawn && from.Coord.file != to.Coord.file);
                san = Letter(type);
                if (type == PieceType.Pawn && capture) san += (char)('a' + from.Coord.file);
                if (type != PieceType.Pawn)
                {
                    var others = state.Board.squares.Where(s => s != from && s.Piece.GetColor() == state.ColorToMove &&
                        s.Piece.GetType() == type && LegalMovesHandler.IsMoveLegal(state, to, s)).ToArray();
                    if (others.Length > 0)
                    {
                        if (others.All(s => s.Coord.file != from.Coord.file)) san += (char)('a' + from.Coord.file);
                        else if (others.All(s => s.Coord.rank != from.Coord.rank)) san += (from.Coord.rank + 1).ToString();
                        else san += from.Coord.ToString();
                    }
                }
                san += (capture ? "x" : "") + to.Coord;
                if (move.Promotion != PieceType.None) san += "=" + Letter(move.Promotion);
            }
            if (!state.TryApplyMove(move)) return null;
            if (state.Result == GameStateUtils.GameResult.Checkmate) san += "#";
            else if (CheckUtils.IsKingInCheck(state.Board, state.ColorToMove)) san += "+";
            return san;
        }
        public static string Variation(GameState position, IReadOnlyList<string> moves, int limit = 6)
        {
            var state = new GameState(); FENUtils.ParseFenString(state, position.CurrentFen);
            state.PositionHistory.AddRange(position.PositionHistory);
            var result = new List<string>();
            foreach (string uci in moves.Take(limit))
            {
                string san = ApplySan(state, uci);
                if (san == null) break;
                result.Add(san);
            }
            return string.Join(" ", result);
        }
        public static string[] LegalUciMoves(GameState state)
        {
            var result = new List<string>();
            if (state.IsGameOver) return result.ToArray();
            foreach (Square from in state.Board.squares.Where(s => s.Piece.GetColor() == state.ColorToMove))
                foreach (Square to in LegalMovesHandler.FindLegalMoves(state, from))
                {
                    string uci = from.Coord.ToString() + to.Coord;
                    if (from.Piece.GetType() == PieceType.Pawn && (to.Coord.rank == 0 || to.Coord.rank == 7))
                        foreach (char suffix in "qrbn") result.Add(uci + suffix);
                    else result.Add(uci);
                }
            return result.ToArray();
        }
    }
}
