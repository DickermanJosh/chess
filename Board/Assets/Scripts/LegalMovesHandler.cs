using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LegalMovesHandler : MonoBehaviour
{
    [Header("Singleton Declaration")] private static LegalMovesHandler _instance;
    public static LegalMovesHandler Instance => _instance;

    private static List<Square> _pseudoLegalMoveList;
    private static int _movesInList;

    [Header("En-Passant Variables")] public static bool CanEnPassant;
    public static int EnPassantMoveNum;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    private void Start()
    {
        _pseudoLegalMoveList = new List<Square>();
    }

    private static List<int> GetMoveDirections(string pieceType)
    {
        var directions = new List<int>();

        switch (pieceType)
        {
            // Directions are represented as the change in the index for each type of move
            case "Rook":
                // Horizontal and Vertical Directions
                directions.Add(8); // Up
                directions.Add(-8); // Down
                directions.Add(1); // Right
                directions.Add(-1); // Left
                break;
            case "Bishop":
                // Diagonal Directions
                directions.Add(9); // Up-Right
                directions.Add(7); // Up-Left
                directions.Add(-7); // Down-Left
                directions.Add(-9); // Down-Right
                break;
            case "Queen":
                // Combines Rook and Bishop Directions
                directions.Add(8); // Up
                directions.Add(-8); // Down
                directions.Add(1); // Right
                directions.Add(-1); // Left
                directions.Add(9); // Up-Right
                directions.Add(7); // Up-Left
                directions.Add(-7); // Down-Left
                directions.Add(-9); // Down-Right
                break;
        }

        return directions;
    }

    public static bool IsPiecesTurn(bool isWhitePiece)
    {
        switch (isWhitePiece)
        {
            case true when MoveTracker.IsWhiteToMove:
            case false when !MoveTracker.IsWhiteToMove:
                return true;
            default:
                return false;
        }
    }

    private static bool IsSquareOccupied(int position)
    {
        return BoardManager.FindSquareFromBoardPos(position).isOccupied;
    }

    private static bool IsOpponentPiece(int position, bool isCurrentPieceWhite)
    {
        // Check if the position is valid and the square is occupied
        if (!IsSquareOccupied(position))
            return false;

        return BoardManager.FindSquareFromBoardPos(position).isPieceWhite != isCurrentPieceWhite;
    }


    private static void FindSlidingPieceMoves(int posInArray, string pieceType, bool isWhite)
    {
        //var moves = new List<Square>();
        var startSquare = BoardManager.FindSquareFromBoardPos(posInArray);
        var startIndex = 0;
        var endIndex = 8;
        
        switch (pieceType)
        {
            case "Rook":
                endIndex = 4;
                break;
            case "Bishop":
                startIndex = 4;
                break;
        }
        
        // Outer loop will go through all the numbers of squares
        // Until the edge of the board, stored by offset.
        for (var i = startIndex; i < endIndex; i++)
        {
            var numUntilEdge = startSquare.numOfSquaresToEdge[i];
            var currentOffset = Square.offsets[i];
            // Inner loop goes over each square individually for each offset
            for (var j = 1; j < numUntilEdge; j++)
            {
                var targetSquare = BoardManager.FindSquareFromBoardPos(posInArray + (currentOffset * j));

                if (targetSquare.isOccupied)
                {
                    // The target square contains an enemy piece, it can be captured but the piece can move no further.
                    if (targetSquare.isPieceWhite != isWhite)
                    { 
                        _pseudoLegalMoveList.Add(BoardManager.Board[targetSquare.BoardPosInArray]);
                        // TODO: This is probably a good spot to look for pins when adding check logic. 
                    }
                    // The target square contains a friendly piece, it cannot be moved to, or passed.
                    break;
                }
                
                // If the target square is not occupied, it can be moved to.
                _pseudoLegalMoveList.Add(BoardManager.Board[targetSquare.BoardPosInArray]);
            } 
        }
        //return moves;
    }

    public static Square[] FindPseudoLegalMoves(string pieceType, bool isWhite, int posInArray, char file, int rank,
        Vector2 pos)
    {
        _pseudoLegalMoveList.Clear();
        _movesInList = 0;
        var fileAsInt = (int)pos.x;
        CanEnPassant = false;

        switch (pieceType)
        {
            case "King":
            {
                //FindPseudoLegalKingMoves(posInArray, fileAsInt, rank, isWhite);
                break;
            }

            case "Queen":
            {
                //CalculateSlidingMoves(BoardManager.Board[posInArray]);
                //FindPseudoLegalRookMoves(posInArray, fileAsInt, rank, isWhite);
                //FindPseudoLegalBishopMoves(posInArray, fileAsInt, rank, isWhite);
                FindSlidingPieceMoves(posInArray, pieceType, isWhite);
                break;
            }

            case "Rook":
            {
                //CalculateSlidingMoves(BoardManager.Board[posInArray]);
                //FindPseudoLegalRookMoves(posInArray, fileAsInt, rank, isWhite);
                FindSlidingPieceMoves(posInArray, pieceType, isWhite);
                break;
            }

            case "Bishop":
            {
                //CalculateSlidingMoves(BoardManager.Board[posInArray]);
                //FindPseudoLegalBishopMoves(posInArray, fileAsInt, rank, isWhite);
                FindSlidingPieceMoves(posInArray, pieceType, isWhite);
                break;
            }

            case "Knight":
            {
                //FindPseudoLegalKnightMoves(posInArray, fileAsInt, rank, isWhite);
                break;
            }

            // White pawns move +8 in the array to move up one or +16 in the array to move up 2
            // +7 && +9 to capture on the diagonals, ONLY for files b - g; a can only be +9 & h +7 otherwise pawns will wrap around the board
            // Reverse the logic for black including a and h
            case "Pawn":
            {
                //FindPseudoLegalPawnMoves(posInArray, fileAsInt, rank, isWhite);
                break;
            }
        }

        // Temp code -= Delete Later =-
        // foreach (var t in PseudoLegalMovesAsInts)
        //{
        //PseudoLegalMoveList.Add(BoardManager.Board[t]);
        //}

        Debug.Log(_pseudoLegalMoveList + " Moves#: " + _movesInList);
        return _pseudoLegalMoveList.ToArray();

        // End FindPseudoLegalMoves
    }
}