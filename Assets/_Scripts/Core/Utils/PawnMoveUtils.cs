using Core;

public static class PawnMoveUtils
{
    public static void FindPseudoLegalPawnMoves(GameState state, int index, int file, int rank, PieceColor color)
    {
        Board board = state.Board;
        int step = color == PieceColor.White ? 8 : -8;
        int next = index + step;
        if (!LegalMovesHandler.IsSquareValid(next)) return;
        if (board.squares[next].Piece.GetType() == PieceType.None)
        {
            LegalMovesHandler.AddMove(board, next);
            bool start = color == PieceColor.White ? rank == 1 : rank == 6;
            if (start && board.squares[index + 2 * step].Piece.GetType() == PieceType.None)
                LegalMovesHandler.AddMove(board, index + 2 * step);
        }
        foreach (int dx in new[] { -1, 1 })
        {
            if (file + dx < 0 || file + dx > 7) continue;
            Square target = board.squares[next + dx];
            if (target.Piece.GetType() != PieceType.None)
            {
                if (target.Piece.GetColor() != color) LegalMovesHandler.AddMove(board, target.Index);
            }
            else if (target.Coord.ToString() == state.EnPassantSquare)
            {
                Piece captured = board.squares[index + dx].Piece;
                bool epRank = color == PieceColor.White ? rank == 4 : rank == 3;
                if (epRank && captured.GetType() == PieceType.Pawn && captured.GetColor() != color)
                    LegalMovesHandler.AddMove(board, target.Index);
            }
        }
    }

    public static void CheckIfMoveAllowsEnPassant(GameState state, Move move)
    {
        state.EnPassantSquare = "-";
        if (move.From.Piece.GetType() == PieceType.Pawn && System.Math.Abs(move.To.Index - move.From.Index) == 16)
            state.EnPassantSquare = state.Board.squares[(move.From.Index + move.To.Index) / 2].Coord.ToString();
    }

    public static void CheckIfMoveWasEnPassant(GameState state, Move move, string previousTarget)
    {
        if (move.From.Piece.GetType() != PieceType.Pawn || move.To.Piece.GetType() != PieceType.None ||
            move.From.Coord.file == move.To.Coord.file || move.To.Coord.ToString() != previousTarget) return;
        int step = move.From.Piece.GetColor() == PieceColor.White ? 8 : -8;
        state.Board.RemovePieceFromSquare(state.Board.squares[move.To.Index - step]);
    }
}
