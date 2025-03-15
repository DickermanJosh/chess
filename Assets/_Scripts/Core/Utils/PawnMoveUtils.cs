using Core;
using UnityEngine;

public static class PawnMoveUtils
{
    public static void FindPseudoLegalPawnMoves(GameState gameState, int posInArray, int file, int rank, PieceColor color)
    {
        Board board = gameState.Board;
        int forwardOffset = (color == PieceColor.Black) ? -8 : 8;
        int leftOffset = (color == PieceColor.Black) ? -7 : 7;
        int rightOffset = (color == PieceColor.Black) ? -9 : 9;

        // If at final rank => promotion logic (stub)
        if (rank == 7 || rank == 0)
        {
            Debug.Log("Pawn is on last rank => would be promotion");
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
        var lastMove = MoveTracker.Instance.GetLastMove();
        if (lastMove == null) return;
        if (lastMove.To == null) return;

        if (lastMove.To.Index != neighborIndex) return;
        if (LegalMovesHandler.GetPieceType(board, neighborIndex) != PieceType.Pawn) return;
        if (!WasTheLastMoveADoublePawnPush(board)) return;

        int enPassantIndex = neighborIndex + forwardOffset;
        if (!LegalMovesHandler.IsSquareValid(enPassantIndex)) return;

        if (!LegalMovesHandler.IsSquareOccupied(board, enPassantIndex))
        {
            LegalMovesHandler.AddMove(board, enPassantIndex);
            gameState.EnPassantSquare = board.squares[enPassantIndex].Coord.ToString();
        }
    }

    private static bool WasTheLastMoveADoublePawnPush(Board board)
    {
        var lastMove = MoveTracker.Instance.GetLastMove();
        if (lastMove == null) return false;

        int from = lastMove.From.Index;
        int to = lastMove.To.Index;

        Square toSquare = board.squares[to];
        if (toSquare.Piece.GetType() != PieceType.Pawn)
            return false;

        // White double push => from == to - 15
        // Black double push => from == to + 15
        if (toSquare.Piece.GetColor() == PieceColor.White && (from == to - 15))
            return true;

        if (toSquare.Piece.GetColor() == PieceColor.Black && (from == to + 15))
            return true;

        return false;
    }
}
