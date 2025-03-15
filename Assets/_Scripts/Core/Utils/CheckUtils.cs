using Core;
using UnityEngine;

public static class CheckUtils
{
    public static bool IsKingInCheck(Board board, PieceColor kingColor)
    {
        // 1) Find the king’s square
        Square kingSquare = null;
        for (int i = 0; i < 64; i++)
        {
            Piece piece = board.squares[i].Piece;
            if (piece.GetType() == PieceType.King && piece.GetColor() == kingColor)
            {
                kingSquare = board.squares[i];
                break;
            }
        }
        if (kingSquare == null)
        {
            // No king found => typically means it was captured 
            // or board state is invalid. We'll just say "not in check."
            return false;
        }

        // 2) Check if any opponent piece can attack the kingSquare
        PieceColor opponentColor = (kingColor == PieceColor.White) ? PieceColor.Black : PieceColor.White;

        // We'll do a partial approach:
        // a) Check for rooks/queens along rank/file
        // b) Check for bishops/queens diagonals
        // c) Check for knights
        // d) Check for pawns
        // e) Check if some king can attack that square (rarely needed unless you want to forbid kings from adjacent squares)

        // We can either do a “sliding scan” for rooks/bishops/queens or do the simpler approach:
        //   - adapt the logic from “FindSlidingPieceMoves” but in reverse, 
        //     see if we hit an opponent rook/bishop/queen in each direction.

        if (IsAttackedBySliding(board, kingSquare, opponentColor))
            return true;

        if (IsAttackedByKnight(board, kingSquare, opponentColor))
            return true;

        if (IsAttackedByPawn(board, kingSquare, opponentColor))
            return true;

        if (IsAttackedByKing(board, kingSquare, opponentColor))
            return true;

        return false;
    }

    /// <summary>
    /// Check if 'kingSquare' is attacked by any opponent rooks/queens along straight lines 
    /// or bishops/queens along diagonals.
    /// </summary>
    private static bool IsAttackedBySliding(Board board, Square kingSquare, PieceColor opponentColor)
    {
        // We can reuse the concept of Board.Offsets: 
        //   first 4 for rook lines, last 4 for bishop diagonals
        // We'll do a total of 8 directions. If we see a rook/queen in directions 0..3, or bishop/queen in directions 4..7, it’s attacking.

        int kingIndex = kingSquare.Index;
        int[] directions = Board.Offsets; // 0..7, 8 directions: N, E, S, W, NE, NW, SE, SW
        int[] slidingStartIndex = { 0, 4 };
        // 0 => rook offsets only (0..3), 4 => bishop offsets only (4..7)

        for (int dirGroup = 0; dirGroup < 2; dirGroup++)
        {
            int start = slidingStartIndex[dirGroup];
            int end = start + 4; // either 0..3 or 4..7
            for (int i = start; i < end; i++)
            {
                // Slide in that direction
                int currentOffset = directions[i];
                int stepsUntilEdge = kingSquare.AvailableDistancesToEdge[i];
                for (int step = 1; step <= stepsUntilEdge; step++)
                {
                    int targetIndex = kingIndex + currentOffset * step;
                    PieceType tPieceType = board.squares[targetIndex].Piece.GetType();
                    PieceColor tPieceColor = board.squares[targetIndex].Piece.GetColor();

                    if (tPieceType == PieceType.None)
                    {
                        // empty => keep scanning
                        continue;
                    }
                    else
                    {
                        // We hit a piece
                        if (tPieceColor == opponentColor)
                        {
                            // If rook or queen is in the rook lines => check
                            // If bishop or queen is in the bishop lines => check
                            bool rookLine = (start == 0); // 0..3
                            if (rookLine && (tPieceType == PieceType.Rook || tPieceType == PieceType.Queen))
                                return true;

                            bool bishopLine = (start == 4); // 4..7
                            if (bishopLine && (tPieceType == PieceType.Bishop || tPieceType == PieceType.Queen))
                                return true;
                        }
                        // either a friendly piece or an irrelevant opponent piece => block further in this direction
                        break;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if 'kingSquare' is attacked by an opponent knight.
    /// Knight offsets are: ±17, ±15, ±10, ±6 (positions on the board).
    /// </summary>
    private static bool IsAttackedByKnight(Board board, Square kingSquare, PieceColor opponentColor)
    {
        // Typical knight offsets from the index perspective:
        int[] knightOffsets = { 17, 15, 10, 6, -17, -15, -10, -6 };
        int kingIndex = kingSquare.Index;

        foreach (int offset in knightOffsets)
        {
            int targetIndex = kingIndex + offset;
            if (targetIndex < 0 || targetIndex >= 64)
                continue;

            // Also need a quick check to avoid “wrapping” across the board 
            // (the file difference must not exceed 2, etc.).
            // A simpler approach is to see if the rank/file difference is valid. 
            // But for brevity, we do minimal checks:
            if (Mathf.Abs((kingIndex % 8) - (targetIndex % 8)) > 2)
                continue;

            // check if there's an opponent knight
            Piece piece = board.squares[targetIndex].Piece;
            if (piece.GetColor() == opponentColor && piece.GetType() == PieceType.Knight)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if 'kingSquare' is attacked by an opponent pawn (i.e. if a pawn could capture 
    /// the king's square).
    /// </summary>
    private static bool IsAttackedByPawn(Board board, Square kingSquare, PieceColor opponentColor)
    {
        // For White pawns, they attack up-left or up-right => offsets: +7, +9 
        // For Black pawns, they attack down-left or down-right => offsets: -7, -9
        int kingIndex = kingSquare.Index;
        bool isWhiteOpponent = (opponentColor == PieceColor.White);

        int[] possiblePawnOffsets = isWhiteOpponent ? new int[] { 7, 9 } : new int[] { -7, -9 };

        foreach (int offset in possiblePawnOffsets)
        {
            int targetIndex = kingIndex + offset;
            if (targetIndex < 0 || targetIndex >= 64)
                continue;

            // check file wrap
            if (Mathf.Abs((kingIndex % 8) - (targetIndex % 8)) != 1)
                continue; // means we jumped across the board in a weird way

            Piece piece = board.squares[targetIndex].Piece;
            if (piece.GetColor() == opponentColor && piece.GetType() == PieceType.Pawn)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if 'kingSquare' is attacked by an opponent king (rare scenario if they are adjacent).
    /// This also helps ensure kings can't stand next to each other illegally.
    /// </summary>
    private static bool IsAttackedByKing(Board board, Square kingSquare, PieceColor opponentColor)
    {
        int kingIndex = kingSquare.Index;
        // The king can move one square in any direction => the offsets from the index perspective:
        int[] kingOffsets = { 8, -8, 1, -1, 9, -9, 7, -7 };

        foreach (int offset in kingOffsets)
        {
            int targetIndex = kingIndex + offset;
            if (targetIndex < 0 || targetIndex >= 64)
                continue;
            if (Mathf.Abs((kingIndex % 8) - (targetIndex % 8)) > 2)
                continue;
            // for a king offset, the file difference can never exceed 1

            Piece piece = board.squares[targetIndex].Piece;
            if (piece.GetColor() == opponentColor && piece.GetType() == PieceType.King)
                return true;
        }
        return false;
    }
}
