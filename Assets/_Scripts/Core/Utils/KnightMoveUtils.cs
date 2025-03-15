using UnityEngine;
using Core;

public static class KnightMoveUtils
{
    public static void FindPseudoLegalKnightMoves(Board board, int posInArray, int file, int rank, PieceColor color)
    {
        bool checkUp = !(rank == 6 || rank == 7);
        bool checkDown = !(rank == 0 || rank == 1);
        bool checkLeft = !(file == 0 || file == 1);
        bool checkRight = !(file == 6 || file == 7);

        if (checkUp)
            CheckKnightDirection(board, posInArray, file, 16, 1, color);

        if (checkDown)
            CheckKnightDirection(board, posInArray, file, -16, 1, color);

        if (checkLeft)
            CheckKnightDirection(board, posInArray, rank, -2, 8, color);

        if (checkRight)
            CheckKnightDirection(board, posInArray, rank, 2, 8, color);
    }

    private static void CheckKnightDirection(Board board, int posInArray, int rankOrFile, int initialOffset, int secondOffset, PieceColor color)
    {
        int feelerSquarePos = posInArray + initialOffset;

        // "Left" direction from that feeler
        if (rankOrFile != 0)
        {
            int targetIndex = feelerSquarePos - secondOffset;
            if (LegalMovesHandler.IsSquareValid(targetIndex))
            {
                if (LegalMovesHandler.IsSquareOccupied(board, targetIndex))
                {
                    if (LegalMovesHandler.GetPieceColor(board, targetIndex) != color)
                        LegalMovesHandler.AddMove(board, targetIndex);
                }
                else
                {
                    LegalMovesHandler.AddMove(board, targetIndex);
                }
            }
        }

        // "Right" direction from that feeler
        if (rankOrFile != 7)
        {
            int targetIndex = feelerSquarePos + secondOffset;
            if (LegalMovesHandler.IsSquareValid(targetIndex))
            {
                if (LegalMovesHandler.IsSquareOccupied(board, targetIndex))
                {
                    if (LegalMovesHandler.GetPieceColor(board, targetIndex) != color)
                        LegalMovesHandler.AddMove(board, targetIndex);
                }
                else
                {
                    LegalMovesHandler.AddMove(board, targetIndex);
                }
            }
        }
    }
}
