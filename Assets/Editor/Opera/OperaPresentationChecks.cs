using System;
using Core;
using UnityEditor;
using UnityEngine;

/// <summary>Engine-independent regression checks for gesture translation.</summary>
public static class OperaPresentationChecks
{
    private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }

    [MenuItem("Opera/Validate presentation rules")]
    public static void Run()
    {
        OperaRulesChecks.Run();
        OperaReviewChecks.Run();
        int checks = 0;
        foreach (var test in new[] {
            ("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1", "e1", "h1", "g1"),
            ("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1", "e1", "a1", "c1"),
            ("r3k2r/8/8/8/8/8/8/R3K2R b KQkq - 0 1", "e8", "h8", "g8"),
            ("r3k2r/8/8/8/8/8/8/R3K2R b KQkq - 0 1", "e8", "a8", "c8"),
            ("r3k2r/8/8/8/8/8/8/R3K2R w Qkq - 0 1", "e1", "h1", "h1"),
            ("r3k2r/8/8/8/8/8/8/R3K2R b KQq - 0 1", "e8", "h8", "h8"),
            (FENUtils.StartFen, "e1", "h1", "h1"),
            ("k3r3/8/8/8/8/8/8/R3K2R w KQ - 0 1", "e1", "h1", "h1"),
            ("k4r2/8/8/8/8/8/8/R3K2R w KQ - 0 1", "e1", "h1", "h1"),
            ("k5r1/8/8/8/8/8/8/R3K2R w KQ - 0 1", "e1", "h1", "h1"),
            ("k2r4/8/8/8/8/8/8/R3K2R w KQ - 0 1", "e1", "a1", "a1"),
            ("k7/8/8/8/8/8/8/RN2K2R w KQ - 0 1", "e1", "a1", "a1"),
            ("k7/8/8/8/8/8/8/R3K2r w KQ - 0 1", "e1", "h1", "h1"),
            ("k7/8/8/8/8/8/8/R2K3R w KQ - 0 1", "d1", "h1", "h1"),
            (FENUtils.StartFen, "e2", "e4", "e4")
        })
        {
            var state = new GameState(); FENUtils.ParseFenString(state, test.Item1);
            var from = state.Board.GetSquareFromNotation(test.Item2);
            var target = state.Board.GetSquareFromNotation(test.Item3);
            var destination = BoardMoveIntent.Destination(state, from, target);
            Require(destination.Coord.ToString() == test.Item4, "Wrong gesture destination: " + test);
            Require(FENUtils.GenerateFen(state) == test.Item1, "Gesture inspection mutated the position.");
            if (test.Item3 != test.Item4)
            {
                var move = new Move(from, destination, state.ColorToMove);
                Require(state.TryApplyMove(move), "Legal rook-click castle rejected.");
                int rookFile = test.Item4[0] == 'g' ? 5 : 3;
                Require(state.Board.GetSquareFromIndex(destination.Coord.rank * 8 + rookFile).Piece.GetType() == PieceType.Rook, "Castling rook did not move.");
                Require(move.ToUci() == test.Item2 + test.Item4, "Nonstandard UCI castling destination.");
            }
            checks++;
        }
        var ep = new GameState(); FENUtils.ParseFenString(ep, "k7/8/8/3pP3/8/8/8/7K w - d6 0 1");
        Require(BoardMoveIntent.IsCapture(ep, ep.Board.GetSquareFromNotation("e5"), ep.Board.GetSquareFromNotation("d6")), "En passant needs a capture marker and sound.");
        var root = new GameObject("Highlight regression");
        try
        {
            var square = ep.Board.GetSquareFromNotation("d6");
            var renderer = root.AddComponent<SquareRenderer>();
            renderer.Init(square, null, Opera.OperaTheme.LightSquare, 1);
            renderer.SetLastMove(true); renderer.AddHighlight(); renderer.SetSelected(true); renderer.RemoveHighlight();
            Require(renderer.IsLastMove && !renderer.IsHighlighted, "Clearing selection erased the last move.");
            renderer.SetLastMove(false); Require(!renderer.IsLastMove, "Last-move state did not clear for a new position.");
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
        Debug.Log("[Opera presentation] " + checks + " gesture cases, en passant marker, and independent highlight states passed.");
    }
}
