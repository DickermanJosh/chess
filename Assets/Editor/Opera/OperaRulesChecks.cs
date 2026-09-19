using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Core;
public static class OperaRulesChecks {
    public static void Run() {
        var state = new GameState();
        string before = state.CurrentFen;
        var moves = LegalMovesHandler.FindLegalMoves(state, state.Board.GetSquareFromNotation("e2"));
        if (moves.Length != 2 || FENUtils.GenerateFen(state) != before)
            throw new Exception("Inspecting moves must preserve the complete starting position");
        UnityEngine.Debug.Log("Read-only move inspection passed");
        int rows = 0;
        foreach (var row in File.ReadAllLines(Path.Combine(UnityEngine.Application.dataPath, "Editor/Opera/RulesPositions.tsv"))) {
            var cells = row.Split('\t');
            state = new GameState(); FENUtils.ParseFenString(state, cells[0]);
            var legal = Moves(state).OrderBy(x => x).ToArray();
            if (string.Join(" ",legal) != cells[1])
                throw new Exception($"Legal move mismatch at {cells[0]}\nActual: {string.Join(" ",legal)}\nExpected: {cells[1]}");
            if (FENUtils.GenerateFen(state) != cells[0]) throw new Exception("Inspection changed position: " + cells[0]);
            if (cells[2] != "-") {
                if (!state.TryApplyMove(Move.FromUci(state,cells[2]))) throw new Exception("Move rejected: " + row);
                if (state.CurrentFen != cells[3]) throw new Exception($"After {cells[2]}: {state.CurrentFen} != {cells[3]}");
            }
            rows++;
        }
        UnityEngine.Debug.Log($"Independent move/FEN fixtures passed: {rows}");
        foreach (var pair in new[] { (FENUtils.StartFen,3,8902L),
            ("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1",2,2039L) }) {
            long count = Perft(pair.Item1,pair.Item2);
            if (count != pair.Item3) throw new Exception($"Perft {pair.Item2}: {count} != {pair.Item3}");
            UnityEngine.Debug.Log($"Perft depth {pair.Item2}: {count}");
        }
        state = new GameState();
        foreach(string move in "g1f3 g8f6 f3g1 f6g8 g1f3 g8f6 f3g1 f6g8".Split(' '))
            if (!state.TryApplyMove(Move.FromUci(state,move))) throw new Exception("Repetition move rejected");
        if (state.Result != GameStateUtils.GameResult.ThreefoldRepetition) throw new Exception("Missing threefold result");
        UnityEngine.Debug.Log("Repetition adjudication passed");
        foreach (var terminal in new[] {
            ("7k/6Q1/6K1/8/8/8/8/8 b - - 0 1", GameStateUtils.GameResult.Checkmate),
            ("7k/5Q2/6K1/8/8/8/8/8 b - - 0 1", GameStateUtils.GameResult.Stalemate),
            ("k7/8/8/8/8/8/8/7K w - - 0 1", GameStateUtils.GameResult.InsufficientMaterial)
        }) {
            state = new GameState(); FENUtils.ParseFenString(state, terminal.Item1);
            if (GameStateUtils.EvaluateGameState(state) != terminal.Item2)
                throw new Exception("Wrong terminal result: " + terminal.Item1);
        }
        state = new GameState(); FENUtils.ParseFenString(state, "7k/8/8/8/8/8/8/R3K3 w - - 99 50");
        if (GameStateUtils.EvaluateGameState(state) != GameStateUtils.GameResult.InProgress ||
            !state.TryApplyMove(Move.FromUci(state, "a1a2")) ||
            state.Result != GameStateUtils.GameResult.FiftyMoveRule)
            throw new Exception("Fifty-move threshold must be 100 halfmoves");
        UnityEngine.Debug.Log("Checkmate, stalemate, material and fifty-move adjudication passed");
    }

    static IEnumerable<string> Moves(GameState state) {
        var moves = new List<string>();
        foreach(var from in state.Board.squares.Where(s=>s.Piece.GetColor()==state.ColorToMove))
            foreach(var to in LegalMovesHandler.FindLegalMoves(state,from)) {
                string root = from.Coord.ToString()+to.Coord.ToString();
                if (from.Piece.GetType()==PieceType.Pawn && (to.Coord.rank==0 || to.Coord.rank==7))
                    foreach(char suffix in "qrbn") moves.Add(root+suffix);
                else moves.Add(root);
            }
        return moves;
    }
    static long Perft(string fen,int depth) {
        if (depth==0) return 1;
        var state=new GameState();FENUtils.ParseFenString(state,fen);
        var moves=Moves(state).ToArray();
        if (depth==1) return moves.Length;
        long sum=0;
        foreach(string move in moves) {
            var child=new GameState();FENUtils.ParseFenString(child,fen);
            if(!child.TryApplyMove(Move.FromUci(child,move)))throw new Exception("Perft move rejected: "+move);
            sum+=Perft(child.CurrentFen,depth-1);
        }
        return sum;
    }
}
