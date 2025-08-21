using Core;
using UnityEngine;

public static class PawnMoveUtils
{
    public static void FindPseudoLegalPawnMoves(GameState gameState, int posInArray, int file, int rank, PieceColor color)
    {
        Board board = gameState.Board;
        int forwardOffset = (color == PieceColor.Black) ? -8 : 8;
        int leftOffset = (color == PieceColor.Black) ? -9 : 7;
        int rightOffset = (color == PieceColor.Black) ? -7 : 9;

        // If at promotion rank, generate promotion moves
        if ((color == PieceColor.White && rank == 7) || (color == PieceColor.Black && rank == 0))
        {
            GeneratePromotionMoves(gameState, posInArray, file, rank, color);
            return;
        }

        // Single push forward
        int forwardIndex = posInArray + forwardOffset;
        if (LegalMovesHandler.IsSquareValid(forwardIndex) && !LegalMovesHandler.IsSquareOccupied(board, forwardIndex))
        {
            LegalMovesHandler.AddMove(board, forwardIndex);
        }

        // Double push from starting rank
        bool atStartingRank = (color == PieceColor.White && rank == 1) || (color == PieceColor.Black && rank == 6);
        if (atStartingRank)
        {
            int doublePushIndex = posInArray + (forwardOffset * 2);
            if (LegalMovesHandler.IsSquareValid(doublePushIndex) &&
                !LegalMovesHandler.IsSquareOccupied(board, doublePushIndex) &&
                !LegalMovesHandler.IsSquareOccupied(board, forwardIndex))
            {
                LegalMovesHandler.AddMove(board, doublePushIndex);
            }
        }

        // Captures
        switch (file)
        {
            case > 0 and < 7:
                PawnCaptureCheck(gameState, posInArray, leftOffset, rank, file, color);
                PawnCaptureCheck(gameState, posInArray, rightOffset, rank, file, color);
                break;
            case 0:
                PawnCaptureCheck(gameState, posInArray, rightOffset, rank, file, color);
                break;
            case 7:
                PawnCaptureCheck(gameState, posInArray, leftOffset, rank, file, color);
                break;
        }
    }

    private static void PawnCaptureCheck(GameState gameState, int posInArray, int directionalOffset, int rank, int file, PieceColor color)
    {
        Board board = gameState.Board;
        int targetIndex = posInArray + directionalOffset;
        if (!LegalMovesHandler.IsSquareValid(targetIndex)) return;

        if (LegalMovesHandler.IsSquareOccupied(board, targetIndex) && LegalMovesHandler.GetPieceColor(board, targetIndex) != color)
        {
            LegalMovesHandler.AddMove(board, targetIndex);
        }

        // Possibly do en passant
        if ((color == PieceColor.White && rank == 4) || (color == PieceColor.Black && rank == 3))
        {
            EnPassantCheck(gameState, posInArray, file, color);
        }
    }

    private static void EnPassantCheck(GameState gameState, int posInArray, int file, PieceColor color)
    {
        int forwardOffset = (color == PieceColor.Black) ? -8 : 8;
        int leftOffset = (color == PieceColor.Black) ? +1 : -1;
        int rightOffset = (color == PieceColor.Black) ? -1 : +1;

        switch (file)
        {
            case > 0 and < 7:
                HandleEnPassantCheck(gameState, posInArray, leftOffset, forwardOffset);
                HandleEnPassantCheck(gameState, posInArray, rightOffset, forwardOffset);
                break;
            case 0:
                HandleEnPassantCheck(gameState, posInArray,
                    (color == PieceColor.White) ? rightOffset : leftOffset,
                    forwardOffset);
                break;
            case 7:
                HandleEnPassantCheck(gameState, posInArray,
                    (color == PieceColor.White) ? leftOffset : rightOffset,
                    forwardOffset);
                break;
        }
    }

    private static void HandleEnPassantCheck(GameState gameState, int posInArray, int fileOffset, int forwardOffset)
    {
        Board board = gameState.Board;
        int neighborIndex = posInArray + fileOffset;
        if (!LegalMovesHandler.IsSquareValid(neighborIndex)) return;

        // Must be a pawn that just moved there
        Move lastMove = gameState.MoveTracker.GetLastMove();

        if (lastMove == null) return;
        if (lastMove.To == null) return;

        if (lastMove.To.Index != neighborIndex) return;
        if (LegalMovesHandler.GetPieceType(board, neighborIndex) != PieceType.Pawn) return;
        if (!WasTheLastMoveADoublePawnPush(gameState)) return;

        int enPassantIndex = neighborIndex + forwardOffset;
        if (!LegalMovesHandler.IsSquareValid(enPassantIndex)) return;

        if (!LegalMovesHandler.IsSquareOccupied(board, enPassantIndex))
        {
            LegalMovesHandler.AddMove(board, enPassantIndex);
            // gameState.EnPassantSquare = board.squares[enPassantIndex].Coord.ToString();
        }
    }

    private static bool WasTheLastMoveADoublePawnPush(GameState gameState)
    {
        Move lastMove = gameState.MoveTracker.GetLastMove();
        if (lastMove == null) return false;

        int from = lastMove.From.Index;
        int to = lastMove.To.Index;

        Square toSquare = gameState.Board.squares[to];
        if (toSquare.Piece.GetType() != PieceType.Pawn)
            return false;

        // White double push => from == to - 15
        // Black double push => from == to + 15
        if (toSquare.Piece.GetColor() == PieceColor.White && (from == to - 16))
            return true;

        if (toSquare.Piece.GetColor() == PieceColor.Black && (from == to + 16))
            return true;

        return false;
    }

    /// <summary>
    /// Checks if a move will result in a position allowing for en passant.
    /// Was the last move a double pawn push and is an opponents pawn next to it
    /// </summary>
    /// <param name="to"></param>
    /// <param name="from"></param>
    public static void CheckIfMoveAllowsEnPassant(GameState gameState, Move move)
    {
        Square from = move.From;
        Square to = move.To;
        if (from.Piece.GetType() != PieceType.Pawn) { return; }
        // If true, the move being made is a double pawn push
        if ((gameState.ColorToMove == PieceColor.White && from.Index + 16 == to.Index) ||
            (gameState.ColorToMove == PieceColor.Black && from.Index - 16 == to.Index))
        {
            // TODO: Check file first
            Square left = gameState.Board.GetSquareFromIndex(to.Index - 1);
            Square right = gameState.Board.GetSquareFromIndex(to.Index + 1);

            // Check if there are enemy pawns adjacent to the pushed pawn
            if (left.Piece.GetType() != PieceType.Pawn && right.Piece.GetType() != PieceType.Pawn) { return; }
            if (gameState.ColorToMove == left.Piece.GetColor() && gameState.ColorToMove == right.Piece.GetColor()) { return; }

            int EnPassantIndex = (gameState.ColorToMove == PieceColor.White) ? to.Index - 8 : to.Index + 8;
            Square EnPassantSquare = gameState.Board.GetSquareFromIndex(EnPassantIndex);
            gameState.EnPassantSquare = EnPassantSquare.Coord.ToString();
        }
    }

    public static void CheckIfMoveWasEnPassant(GameState gameState, Move move, string EnPassantSquare)
    {
        Square from = move.From;
        Square to = move.To;
        // Check if the move is En Passant, remove the opponent's pawn if it was
        Debug.Log($"EPSquare: {EnPassantSquare}. To Coord: {to.Coord.ToString()}");
        if (EnPassantSquare.Equals("-")) { return; }
        if (!to.Coord.ToString().Equals(EnPassantSquare)) { return; }
        Debug.Log($"En Passant taken.");

        PieceColor col = from.Piece.GetColor();
        int index = to.Index;

        // White => -8 to move back behind the pushed En Passant pawn
        index = (col == PieceColor.White) ? index - 8 : index + 8;
        // if (col == PieceColor.White) { index -= 8; }
        // // Black +8 to move back behind the pushed pawn
        // else { index += 8; }

        Square opponentPawn = gameState.Board.GetSquareFromIndex(index);
        gameState.Board.RemovePieceFromSquare(opponentPawn);
        Debug.Log($"Opponent pawn removed at index {index}");
    }

    private static void GeneratePromotionMoves(GameState gameState, int posInArray, int file, int rank, PieceColor color)
    {
        Board board = gameState.Board;
        int forwardOffset = (color == PieceColor.Black) ? -8 : 8;
        int leftOffset = (color == PieceColor.Black) ? -9 : 7;
        int rightOffset = (color == PieceColor.Black) ? -7 : 9;

        // Forward promotion (if square is empty)
        int forwardIndex = posInArray + forwardOffset;
        if (LegalMovesHandler.IsSquareValid(forwardIndex) && !LegalMovesHandler.IsSquareOccupied(board, forwardIndex))
        {
            AddPromotionMoves(board, forwardIndex);
        }

        // Capture promotions
        switch (file)
        {
            case > 0 and < 7:
                CheckPromotionCapture(board, posInArray, leftOffset, color);
                CheckPromotionCapture(board, posInArray, rightOffset, color);
                break;
            case 0:
                CheckPromotionCapture(board, posInArray, rightOffset, color);
                break;
            case 7:
                CheckPromotionCapture(board, posInArray, leftOffset, color);
                break;
        }
    }

    private static void CheckPromotionCapture(Board board, int posInArray, int directionalOffset, PieceColor color)
    {
        int targetIndex = posInArray + directionalOffset;
        if (!LegalMovesHandler.IsSquareValid(targetIndex)) return;

        if (LegalMovesHandler.IsSquareOccupied(board, targetIndex) && LegalMovesHandler.GetPieceColor(board, targetIndex) != color)
        {
            AddPromotionMoves(board, targetIndex);
        }
    }

    private static void AddPromotionMoves(Board board, int targetIndex)
    {
        // For now, we'll just add a single move to the square
        // In a complete implementation, you'd want to track what piece type the promotion is to
        // This could be handled by extending the Square class or creating a special PromotionMove class
        LegalMovesHandler.AddMove(board, targetIndex);
    }
}
