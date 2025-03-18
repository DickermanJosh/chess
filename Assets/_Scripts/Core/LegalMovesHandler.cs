using Core;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Responsible for generating pseudo-legal moves for pieces. 
/// Contains logic for sliding moves, knight moves, pawn moves, etc.
/// This script is independent of GameManager so it can be used both client-side and server-side.
/// </summary>
public class LegalMovesHandler : MonoBehaviour
{
    [Header("Singleton Declaration")]
    private static LegalMovesHandler _instance;
    public static LegalMovesHandler Instance => _instance;

    // We store temporary pseudo-legal moves here.
    private static List<Square> _pseudoLegalMoveList;
    private static int _movesInPseudoLegalList;
    private static List<Square> _legalMoveList;
    private static int _movesInLegalList;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    private void Start()
    {
        _pseudoLegalMoveList = new List<Square>();
        _legalMoveList = new List<Square>();
    }

    public static bool IsMoveLegal(GameState gameState, Square to, Square from)
    {
        Square[] legalMoves = FindLegalMoves(gameState, from);

        foreach(var move in legalMoves)
        {
            if (move.Equals(to))
            {
                return true;
            }
        }

        return false;

    }

    public static Square[] FindLegalMoves(GameState gameState, Square sq)
    {
        _legalMoveList.Clear();
        _movesInLegalList = 0;
        GameState gameStateCopy = gameState;

        Square[] pseudos = FindPseudoLegalMoves(gameStateCopy, sq);

        return pseudos;
        // if (pseudos.Length == 0) { return pseudos; }


        // foreach (var move in pseudos)
        // {
        //     Board temp = gameStateCopy.Board.Clone();
        //     temp.ApplyMove(sq, move);

        //     if (!CheckUtils.IsKingInCheck(temp, sq.Piece.GetColor()))
        //     {
        //         //_pseudoLegalMoveList.Remove(move);
        //         _legalMoveList.Add(move);
        //     }
        // }

        // return _legalMoveList.ToArray();
    }

    /// <summary>
    /// Main entry for generating pseudo-legal moves for the piece on Square sq, on the given board.
    /// Currently incomplete for King, etc.
    /// </summary>
    /// <param name="board">The board to use for generating moves</param>
    /// <param name="sq">The square containing the piece whose moves we're finding</param>
    /// <returns>Array of squares this piece can move to (pseudo-legal)</returns>
    private static Square[] FindPseudoLegalMoves(GameState gameState, Square sq)
    {
        // Clear from previous usage
        _pseudoLegalMoveList.Clear();
        _movesInPseudoLegalList = 0;

        gameState.EnPassantSquare = "-";

        int posInArray = sq.Index;
        Board board = gameState.Board;
        PieceType pieceType = sq.Piece.GetType();
        PieceColor color = sq.Piece.GetColor();
        // or pass in color as a separate param if needed

        switch (pieceType)
        {
            case PieceType.King:
                KingMoveUtils.FindPseudoLegalKingMoves(gameState, color);
                break;

            case PieceType.Queen:
            case PieceType.Rook:
            case PieceType.Bishop:
                FindSlidingPieceMoves(board, posInArray, pieceType, color);
                break;

            case PieceType.Knight:
                {
                    int file = sq.Coord.file;
                    int rank = sq.Coord.rank;
                    KnightMoveUtils.FindPseudoLegalKnightMoves(board, posInArray, file, rank, color);
                    break;
                }

            case PieceType.Pawn:
                {
                    int file = sq.Coord.file;
                    int rank = sq.Coord.rank;
                    PawnMoveUtils.FindPseudoLegalPawnMoves(gameState, posInArray, file, rank, color);
                    break;
                }

            case PieceType.None:
                // No piece => no moves
                break;
        }

        Debug.Log($"[LegalMovesHandler] Found {_movesInPseudoLegalList} pseudo-legal moves for {pieceType} at index {posInArray}.");
        return _pseudoLegalMoveList.ToArray();
    }

    #region Sliding Pieces (Rook, Bishop, Queen)

    private static void FindSlidingPieceMoves(Board board, int posInArray, PieceType pieceType, PieceColor color)
    {
        Square startSquare = board.GetSquareFromIndex(posInArray);

        int startIndex = 0;
        int endIndex = 8; // 0..7 = all 8 directions

        switch (pieceType)
        {
            case PieceType.Rook:
                endIndex = 4; // Rook => only first 4 directions (N,S,E,W)
                break;
            case PieceType.Bishop:
                startIndex = 4; // Bishop => last 4 directions (diagonals)
                break;
                // Queen => uses all 8 directions
        }

        // Move along each direction
        for (int i = startIndex; i < endIndex; i++)
        {
            int numUntilEdge = startSquare.AvailableDistancesToEdge[i];
            int currentOffset = Board.Offsets[i];

            for (int j = 1; j <= numUntilEdge; j++)
            {
                int targetIndex = posInArray + currentOffset * j;

                if (IsSquareOccupied(board, targetIndex))
                {
                    // Occupied => can capture if it's an opponent
                    if (GetPieceColor(board, targetIndex) != color)
                    {
                        AddMove(board, targetIndex);
                    }
                    // Stop searching further in this direction
                    break;
                }
                // Not occupied => can move here, keep going
                AddMove(board, targetIndex);
            }
        }
    }

    #endregion

    /// <summary>
    /// Checks if a piece with color is white or black matches the sideToMove 
    /// (no references to GameManager).
    /// </summary>
    public static bool IsPiecesTurn(bool pieceIsWhite, PieceColor colorToMove)
    {
        // If colorToMove is White, piece must be white
        // If colorToMove is Black, piece must be black
        if (pieceIsWhite && colorToMove == PieceColor.White) return true;
        if (!pieceIsWhite && colorToMove == PieceColor.Black) return true;
        return false;
    }

    public static bool IsSquareOccupied(Board board, int position)
    {
        return GetPieceType(board, position) != PieceType.None;
    }

    public static bool IsSquareValid(int position)
    {
        return position >= 0 && position < 64;
    }

    public static bool IsOpponentPiece(Board board, int position, PieceColor color)
    {
        return GetPieceType(board, position) != PieceType.None
               && (GetPieceColor(board, position) != color);
    }

    /// <summary>
    /// Retrieves the PieceType at the given index from the given board.
    /// </summary>
    public static PieceType GetPieceType(Board board, int index)
    {
        return board.squares[index].Piece.GetType();
    }

    /// <summary>
    /// Retrieves the PieceColor at the given index from the given board.
    /// </summary>
    public static PieceColor GetPieceColor(Board board, int index)
    {
        return board.squares[index].Piece.GetColor();
    }

    /// <summary>
    /// Adds the square at targetIndex to the pseudo-legal move list if valid.
    /// </summary>
    public static void AddMove(Board board, int targetIndex)
    {
        _pseudoLegalMoveList.Add(board.squares[targetIndex]);
        _movesInPseudoLegalList++;
    }
}
