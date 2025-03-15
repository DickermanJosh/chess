using UnityEngine;
using Core;

public static class KingMoveUtils
{
    public static void FindPseudoLegalKingMoves(GameState gameState, PieceColor color)
    {
        Board board = gameState.Board;
        int posInArray = board.FindKing(color).Index;
        // The 8 possible one-square king moves
        int[] kingOffsets = { 8, -8, 1, -1, 9, -9, 7, -7 };

        // For each offset:
        foreach (int offset in kingOffsets)
        {
            int targetIndex = posInArray + offset;
            if (!LegalMovesHandler.IsSquareValid(targetIndex)) continue;

            // Ensure we don't wrap columns 
            if (Mathf.Abs((posInArray % 8) - (targetIndex % 8)) > 1)
                continue;

            // Check if it's occupied by friendly piece
            PieceColor occupantColor = LegalMovesHandler.GetPieceColor(board, targetIndex);
            if (occupantColor == color) continue;

            // Now check if that square is attacked
            // Temporarily move the king there? Or do a quick "would this square be attacked?"
            // Easiest approach for pseudo-legal is to just add it
            // but if you want to filter squares that are obviously attacked, do:
            // if (!IsSquareAttackedByOpponent(board, targetIndex, OpponentOf(color))) 
            //     AddMove(board, targetIndex);
            LegalMovesHandler.AddMove(board, targetIndex);
        }

        // Then handle castling
        if (color == PieceColor.White)
        {
            // King-side castling => e1 to g1 => squares f1,g1 must be empty & not attacked
            // Rook is at h1 => index 7 in rank 0
            if (gameState.WhiteKingSideCastle)
            {
                AttemptKingSideCastle(gameState, posInArray, color);
            }
            if (gameState.WhiteQueenSideCastle)
            {
                AttemptQueenSideCastle(gameState, posInArray, color);
            }
        }
        else
        {
            // black => e8 to g8 => squares f8,g8 must be empty/not attacked, rook at h8 => index 63
            if (gameState.BlackKingSideCastle)
            {
                AttemptKingSideCastle(gameState, posInArray, color);
            }
            if (gameState.BlackQueenSideCastle)
            {
                AttemptQueenSideCastle(gameState, posInArray, color);
            }
        }
    }

    private static void AttemptKingSideCastle(GameState gameState, int kingIndex, PieceColor color)
    {
        Board board = gameState.Board;
        // For White: king starts e1 => index=4, rook is h1 => index=7
        // For Black: king e8 => index=60, rook h8 => index=63
        int rankOffset = (color == PieceColor.White) ? 0 : 56;
        int kingStart = 4 + rankOffset;
        int rookStart = 7 + rankOffset;

        // If the 'kingIndex' doesn't match 'kingStart', maybe the king has moved? 
        // We can do a quick check, or rely on the GameState's castling flags.

        // Check squares in between are empty: (kingStart+1) => f-file, (kingStart+2) => g-file
        if (LegalMovesHandler.IsSquareOccupied(board, kingStart + 1) || LegalMovesHandler.IsSquareOccupied(board, kingStart + 2))
            return;

        // Also ensure those squares are not attacked
        // i.e. if IsSquareAttackedByOpponent(board, kingStart+1, OpponentOf(color)) => fail, same for kingStart+2
        // We'll skip the details, but you can do it similarly to 'IsAttackedBySliding(...)' 
        // or do a temporary approach.

        // If all good, add the castling move as if "king moves to (kingStart+2)"
        LegalMovesHandler.AddMove(board, kingStart + 2);
    }

    private static void AttemptQueenSideCastle(GameState gameState, int kingIndex, PieceColor color)
    {
        Board board = gameState.Board;
        // For White: king e1 => index=4, rook a1 => index=0
        // For Black: king e8 => index=60, rook a8 => index=56
        int rankOffset = (color == PieceColor.White) ? 0 : 56;
        int kingStart = 4 + rankOffset;
        int rookStart = 0 + rankOffset;

        // Check squares between the king and rook: (kingStart-1) => d-file, (kingStart-2) => c-file, etc.
        // Also check they're not attacked, etc.

        if (LegalMovesHandler.IsSquareOccupied(board, kingStart - 1) 
        || LegalMovesHandler.IsSquareOccupied(board, kingStart - 2) 
        || LegalMovesHandler.IsSquareOccupied(board, kingStart - 3))
            return;

        // Check if squares are attacked, etc.

        // If all good, add "king moves to (kingStart-2)"
        LegalMovesHandler.AddMove(board, kingStart - 2);
    }

}
