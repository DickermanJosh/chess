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

        // Check to make sure the rook is still on its start square
        Square rookStartSq = board.GetSquareFromIndex(rookStart);
        if (rookStartSq.Piece.GetType() != PieceType.Rook || rookStartSq.Piece.GetColor() != color) { return; }

        // If the 'kingIndex' doesn't match 'kingStart' the king has moved 
        if (kingIndex != kingStart) { return; }

        // Check squares in between are empty: (kingStart+1) => f-file, (kingStart+2) => g-file
        if (LegalMovesHandler.IsSquareOccupied(board, kingStart + 1) || LegalMovesHandler.IsSquareOccupied(board, kingStart + 2)) { return; }

        // Also ensure those squares are not attacked
        // i.e. if IsSquareAttackedByOpponent(board, kingStart+1, OpponentOf(color)) => fail, same for kingStart+2
        PieceColor opponentColor = (color == PieceColor.White) ? PieceColor.Black : PieceColor.White;
        Square kingSquarePlus1 = board.GetSquareFromIndex(kingStart + 1);
        Square kingSquarePlus2 = board.GetSquareFromIndex(kingStart + 2);
        if (CheckUtils.IsSquareAttackedByOpponent(board, kingSquarePlus1, opponentColor)) { return; }
        if (CheckUtils.IsSquareAttackedByOpponent(board, kingSquarePlus2, opponentColor)) { return; }


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


    /// <summary>
    /// Checks the given move to see if it was a king castling.
    /// Handled moving the rook and updating castling rights
    /// </summary>
    /// <param name="gameState"></param>
    /// <param name="move"></param>
    public static void CheckIfMoveWasCastle(GameState gameState, Move move)
    {
        Square from = move.From;
        Square to = move.To;

        if (from.Piece.GetType() != PieceType.King) { return; }

        PieceColor kingColor = from.Piece.GetColor();

        // Kingside castle
        if (from.Index == to.Index - 2) { CastleRook(gameState, kingColor, true); }

        // Queenside castle
        if (from.Index == to.Index + 2) { CastleRook(gameState, kingColor, false); }
    }

    /// <summary>
    /// Moves the rook into its proper spot when the king castles.
    /// Meant to be called after confirming the move being made is a castle
    /// </summary>
    private static void CastleRook(GameState gameState, PieceColor color, bool kingSide)
    {
        Board board = gameState.Board;
        if (kingSide && color == PieceColor.White)
        {
            Square to = board.GetSquareFromIndex(5);
            Square from = board.GetSquareFromIndex(7);
            board.ApplyMove(from, to, gameState.EnPassantSquare);
        }
        else if (kingSide && color == PieceColor.Black)
        {
            Square to = board.GetSquareFromIndex(61);
            Square from = board.GetSquareFromIndex(63);
            board.ApplyMove(from, to, gameState.EnPassantSquare);
        }
        else if (!kingSide && color == PieceColor.White)
        {
            Square to = board.GetSquareFromIndex(3);
            Square from = board.GetSquareFromIndex(0);
            board.ApplyMove(from, to, gameState.EnPassantSquare);
        }
        else
        {
            Square to = board.GetSquareFromIndex(59);
            Square from = board.GetSquareFromIndex(56);
            board.ApplyMove(from, to, gameState.EnPassantSquare);
        }

        gameState.Board = board;

        if (color == PieceColor.White)
        {
            gameState.WhiteKingSideCastle = false;
            gameState.WhiteQueenSideCastle = false;
        }
        else
        {
            gameState.BlackKingSideCastle = false;
            gameState.BlackQueenSideCastle = false;
        }

    }

}
