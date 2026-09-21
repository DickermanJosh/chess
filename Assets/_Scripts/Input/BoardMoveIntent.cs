using Core;

/// <summary>Translates a gesture into a normal, rules-validated chess move.</summary>
public static class BoardMoveIntent
{
    public static Square Destination(GameState state, Square from, Square target)
    {
        if (state == null || from == null || target == null) return null;
        if (from.Piece.GetType() != PieceType.King || target.Piece.GetType() != PieceType.Rook ||
            from.Piece.GetColor() != target.Piece.GetColor()) return target;
        int rank = from.Piece.GetColor() == PieceColor.White ? 0 : 7;
        if (from.Coord.file != 4 || from.Coord.rank != rank || target.Coord.rank != rank ||
            (target.Coord.file != 0 && target.Coord.file != 7)) return target;
        var destination = state.Board.GetSquareFromIndex(rank * 8 + (target.Coord.file == 7 ? 6 : 2));
        // Rights, check, and attacked transit squares stay with the shared rules.
        // The engine and server always receive the standard king destination.
        return LegalMovesHandler.IsMoveLegal(state, destination, from) ? destination : target;
    }

    public static bool IsCapture(GameState state, Square from, Square to) =>
        to.Piece.GetType() != PieceType.None ||
        (from.Piece.GetType() == PieceType.Pawn && from.Coord.file != to.Coord.file &&
         to.Coord.ToString() == state.EnPassantSquare);
}
