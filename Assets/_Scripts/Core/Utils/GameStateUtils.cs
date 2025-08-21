using Core;
using System.Collections.Generic;
using UnityEngine;

public static class GameStateUtils
{
    public enum GameResult
    {
        InProgress,
        Checkmate,
        Stalemate,
        InsufficientMaterial,
        FiftyMoveRule,
        ThreefoldRepetition
    }

    public static GameResult EvaluateGameState(GameState gameState)
    {
        PieceColor currentPlayer = gameState.ColorToMove;
        bool isInCheck = CheckUtils.IsKingInCheck(gameState.Board, currentPlayer);
        bool hasLegalMoves = HasAnyLegalMoves(gameState, currentPlayer);

        if (!hasLegalMoves)
        {
            return isInCheck ? GameResult.Checkmate : GameResult.Stalemate;
        }

        if (IsInsufficientMaterial(gameState.Board))
        {
            return GameResult.InsufficientMaterial;
        }

        if (IsFiftyMoveRule(gameState))
        {
            return GameResult.FiftyMoveRule;
        }

        if (IsThreefoldRepetition(gameState))
        {
            return GameResult.ThreefoldRepetition;
        }

        return GameResult.InProgress;
    }

    private static bool HasAnyLegalMoves(GameState gameState, PieceColor color)
    {
        Board board = gameState.Board;
        
        for (int i = 0; i < 64; i++)
        {
            Square square = board.squares[i];
            if (square.Piece.GetType() != PieceType.None && square.Piece.GetColor() == color)
            {
                Square[] legalMoves = LegalMovesHandler.FindLegalMoves(gameState, square);
                if (legalMoves.Length > 0)
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    private static bool IsInsufficientMaterial(Board board)
    {
        List<PieceType> whitePieces = new List<PieceType>();
        List<PieceType> blackPieces = new List<PieceType>();

        for (int i = 0; i < 64; i++)
        {
            Piece piece = board.squares[i].Piece;
            if (piece.GetType() != PieceType.None && piece.GetType() != PieceType.King)
            {
                if (piece.GetColor() == PieceColor.White)
                    whitePieces.Add(piece.GetType());
                else
                    blackPieces.Add(piece.GetType());
            }
        }

        // King vs King
        if (whitePieces.Count == 0 && blackPieces.Count == 0)
            return true;

        // King and Bishop vs King
        if ((whitePieces.Count == 1 && whitePieces[0] == PieceType.Bishop && blackPieces.Count == 0) ||
            (blackPieces.Count == 1 && blackPieces[0] == PieceType.Bishop && whitePieces.Count == 0))
            return true;

        // King and Knight vs King
        if ((whitePieces.Count == 1 && whitePieces[0] == PieceType.Knight && blackPieces.Count == 0) ||
            (blackPieces.Count == 1 && blackPieces[0] == PieceType.Knight && whitePieces.Count == 0))
            return true;

        // King and Bishop vs King and Bishop (same color squares)
        if (whitePieces.Count == 1 && blackPieces.Count == 1 &&
            whitePieces[0] == PieceType.Bishop && blackPieces[0] == PieceType.Bishop)
        {
            return AreBishopsOnSameColorSquares(board);
        }

        return false;
    }

    private static bool AreBishopsOnSameColorSquares(Board board)
    {
        bool? whiteBishopOnWhiteSquare = null;
        bool? blackBishopOnWhiteSquare = null;

        for (int i = 0; i < 64; i++)
        {
            Piece piece = board.squares[i].Piece;
            if (piece.GetType() == PieceType.Bishop)
            {
                bool isOnWhiteSquare = board.squares[i].IsWhite;
                if (piece.GetColor() == PieceColor.White)
                    whiteBishopOnWhiteSquare = isOnWhiteSquare;
                else
                    blackBishopOnWhiteSquare = isOnWhiteSquare;
            }
        }

        return whiteBishopOnWhiteSquare == blackBishopOnWhiteSquare;
    }

    private static bool IsFiftyMoveRule(GameState gameState)
    {
        return gameState.HalfMoveClock >= 50;
    }

    private static bool IsThreefoldRepetition(GameState gameState)
    {
        if (gameState.PositionHistory.Count < 6) // Need at least 3 occurrences
            return false;

        string currentPosition = gameState.CurrentFen;
        int occurrences = 1; // Current position counts as 1

        foreach (string position in gameState.PositionHistory)
        {
            if (position == currentPosition)
            {
                occurrences++;
                if (occurrences >= 3)
                    return true;
            }
        }

        return false;
    }
}