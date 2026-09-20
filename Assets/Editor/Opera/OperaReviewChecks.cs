using System;
using System.IO;
using System.Linq;
using Core;
using Opera;
using UnityEngine;

public static class OperaReviewChecks
{
    private static void Require(bool value, string error) { if (!value) throw new Exception(error); }
    public static void Run()
    {
        int fixtures = 0;
        foreach (string row in File.ReadAllLines(Path.Combine(Application.dataPath, "Editor/Opera/NotationPositions.tsv")))
        {
            string[] cells = row.Split('\t');
            var position = new GameState(); FENUtils.ParseFenString(position, cells[0]);
            string san = ChessNotation.ApplySan(position, cells[1]);
            Require(san == cells[2], "SAN mismatch: " + cells[0] + " " + cells[1] + " expected " + cells[2] + ", got " + san);
            Require(position.CurrentFen == cells[3], "SAN application changed the resulting position."); fixtures++;
        }
        var record = new OperaGameRecord(PieceColor.White) { EngineName = "Opera \"test\"", EngineRevision = "test", EngineHash = "test" };
        foreach (string move in "e2e4 e7e5 g1f3 b8c6 f1c4 g8f6 e1g1".Split(' ')) Require(record.TryMove(move), "Opening move failed.");
        string live = record.Live.CurrentFen;
        var snapshot = record.PositionAt(2);
        Require(record.Live.CurrentFen == live && record.Plies.Count == 7 && snapshot.WhiteKingSideCastle, "Review changed the live game or castling rights.");
        string original = record.ExportPgn(2);
        Require(original.Contains("3. Bc4 Nf6") && original.Contains("4. O-O") && original.Contains("[ViewedPly \"2\"]"), "Missing complete SAN transcript or viewed position: " + original);
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "opera-review-export.pgn"), original);
        record.ResumeAt(2);
        Require(record.Plies.Count == 2 && record.Live.CurrentFen == snapshot.CurrentFen && record.Live.PositionHistory.Count == 2, "Resume lost history.");
        Require(record.TryMove("f1c4") && record.Plies.Count == 3, "Could not branch from reviewed position.");
        Require(!record.ExportPgn(3).Contains("O-O"), "Old continuation survived resume.");
        record.Resign(); Require(record.Result == "0-1" && record.Live.IsGameOver, "Resignation result lost.");
        record.ResumeAt(2); Require(record.Result == "*" && !record.Live.IsGameOver, "Resume did not clear resignation.");
        var repetition = new OperaGameRecord(PieceColor.Black);
        foreach (string move in "g1f3 g8f6 f3g1 f6g8 g1f3 g8f6 f3g1 f6g8".Split(' ')) Require(repetition.TryMove(move), "Repetition move failed.");
        Require(repetition.Result == "1/2-1/2", "Repetition not recorded.");
        repetition.ResumeAt(4);
        foreach (string move in "g1f3 g8f6 f3g1 f6g8".Split(' ')) Require(repetition.TryMove(move), "Replay lost repetition.");
        Require(repetition.Live.Result == GameStateUtils.GameResult.ThreefoldRepetition, "Resume lost earlier repetition keys.");
        var ep = new OperaGameRecord(PieceColor.White);
        foreach (string move in "e2e4 a7a6 e4e5 d7d5 e5d6".Split(' ')) Require(ep.TryMove(move), "EP sequence failed.");
        ep.ResumeAt(4); Require(ep.Live.EnPassantSquare == "d6" && ep.TryMove("e5d6"), "Resume lost en passant.");
        Require(ep.Plies[4].San == "exd6", "Wrong en passant SAN.");
        var mate = new OperaGameRecord(PieceColor.White);
        foreach (string move in "f2f3 e7e5 g2g4 d8h4".Split(' ')) Require(mate.TryMove(move), "Mate sequence failed.");
        Require(mate.Plies[3].San == "Qh4#" && mate.Result == "0-1", "Missing mate suffix or result.");
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "opera-review-mate.pgn"), mate.ExportPgn(4));
        var cp = UciSearchInfo.Parse("info nodes 50 depth 8 score cp 62 pv e7e5 g1f3");
        Require(cp != null && cp.Depth == 8 && cp.WhiteLabel(false) == "-0.62" && cp.WhiteLabel(true) == "+0.62", "Score perspective is wrong.");
        var forcedMate = UciSearchInfo.Parse("info depth 6 score mate -2 pv e7e5");
        Require(forcedMate.WhiteLabel(false) == "+M2" && forcedMate.WhiteLabel(true) == "-M2", "Mate perspective is wrong.");
        Require(UciSearchInfo.Parse("info string score cp 90") == null, "Diagnostic text parsed as a score.");
        Require(UciSearchInfo.Parse("info depth 4 score cp 90 lowerbound pv e2e4").IsBound, "Bound lost.");
        Require(UciSearchInfo.Parse("info depth 4 pv e2e4") == null, "Missing score accepted.");
        Debug.Log("[Opera checks] " + fixtures + " independent SAN fixtures; review, resume, PGN, castling, en passant, repetition, mate, resignation and score perspective passed.");
    }
}
