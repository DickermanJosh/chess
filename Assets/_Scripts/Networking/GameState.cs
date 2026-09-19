using Core;
using System;
using System.Collections.Generic;

public class GameState
{
    public Board Board { get; set; }
    public PieceColor ColorToMove { get; set; }
    public bool IsGameOver { get; set; }
    public int HalfMoves { get; set; }
    public int FullMoves { get; set; }
    public bool WhiteKingSideCastle { get; set; }
    public bool WhiteQueenSideCastle { get; set; }
    public bool BlackKingSideCastle { get; set; }
    public bool BlackQueenSideCastle { get; set; }
    public string EnPassantSquare { get; set; }
    public string CurrentFen { get; set; }
    public MoveTracker MoveTracker { get; private set; }
    public int HalfMoveClock { get; set; }
    // Canonical position keys, without clocks and without uncapturable EP targets.
    public List<string> PositionHistory { get; private set; }
    public GameStateUtils.GameResult Result { get; private set; }

    public GameState()
    {
        Board = new Board(64);
        Board.Init();
        Board.LoadPiecesFromFen(FENUtils.StartFen);
        CurrentFen = FENUtils.StartFen;
        ColorToMove = PieceColor.White;
        FullMoves = 1;
        WhiteKingSideCastle = WhiteQueenSideCastle = true;
        BlackKingSideCastle = BlackQueenSideCastle = true;
        EnPassantSquare = "-";
        MoveTracker = new MoveTracker();
        PositionHistory = new List<string>();
    }

    // Preserve the existing server entrypoint while sharing the same local rules.
    public void ValidateUpdateAndAppleMove(Move move) => TryApplyMove(move);

    public bool TryApplyMove(Move move)
    {
        if (IsGameOver || move?.From == null || move.To == null || move.From == move.To) return false;
        Square from = Board.GetSquareFromIndex(move.From.Index);
        Square to = Board.GetSquareFromIndex(move.To.Index);
        if (from == null || to == null || from.Piece.GetColor() != ColorToMove ||
            !LegalMovesHandler.IsMoveLegal(this, to, from)) return false;
        move.From = from;
        move.To = to;
        bool pawn = from.Piece.GetType() == PieceType.Pawn;
        bool promotion = pawn && (to.Coord.rank == 0 || to.Coord.rank == 7);
        if (promotion && move.Promotion == PieceType.None) move.Promotion = PieceType.Queen;
        if (promotion ? !IsPromotionPiece(move.Promotion) : move.Promotion != PieceType.None) return false;

        PositionHistory.Add(GameStateUtils.PositionKey(this));
        bool capture = to.Piece.GetType() != PieceType.None;
        HalfMoveClock = pawn || capture ? 0 : HalfMoveClock + 1;
        string ep = EnPassantSquare;
        PieceColor color = ColorToMove;
        if (from.Piece.GetType() == PieceType.King)
        {
            if (color == PieceColor.White) WhiteKingSideCastle = WhiteQueenSideCastle = false;
            else BlackKingSideCastle = BlackQueenSideCastle = false;
        }
        if (from.Piece.GetType() == PieceType.Rook) RemoveRookRight(from.Index);
        if (to.Piece.GetType() == PieceType.Rook) RemoveRookRight(to.Index);
        PawnMoveUtils.CheckIfMoveAllowsEnPassant(this, move);
        PawnMoveUtils.CheckIfMoveWasEnPassant(this, move, ep);
        KingMoveUtils.CheckIfMoveWasCastle(this, move);
        Board.ApplyMove(from, to, ep);
        if (promotion) to.Piece = new Piece(move.Promotion, color);
        MoveTracker.AddMove(move);
        UpdateMoveOrder();
        CurrentFen = FENUtils.GenerateFen(this);
        Result = GameStateUtils.EvaluateGameState(this);
        IsGameOver = Result != GameStateUtils.GameResult.InProgress;
        return true;
    }

    private static bool IsPromotionPiece(PieceType p) => p == PieceType.Queen || p == PieceType.Rook ||
        p == PieceType.Bishop || p == PieceType.Knight;

    private void RemoveRookRight(int index)
    {
        switch (index)
        {
            case 0: WhiteQueenSideCastle = false; break;
            case 7: WhiteKingSideCastle = false; break;
            case 56: BlackQueenSideCastle = false; break;
            case 63: BlackKingSideCastle = false; break;
        }
    }

    public void UpdateMoveOrder()
    {
        HalfMoves++;
        if (ColorToMove == PieceColor.Black) FullMoves++;
        ColorToMove = ColorToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
    }
}
