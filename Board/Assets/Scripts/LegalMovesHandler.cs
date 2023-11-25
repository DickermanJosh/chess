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
                FindSlidingPieceMoves(posInArray, pieceType, isWhite);
                break;
            }

            case "Rook":
            {
                FindSlidingPieceMoves(posInArray, pieceType, isWhite);
                break;
            }

            case "Bishop":
            {
                FindSlidingPieceMoves(posInArray, pieceType, isWhite);
                break;
            }

            case "Knight":
            {
                FindPseudoLegalKnightMoves(posInArray, fileAsInt, rank, isWhite);
                break;
            }

            // White pawns move +8 in the array to move up one or +16 in the array to move up 2
            // +7 && +9 to capture on the diagonals, ONLY for files b - g; a can only be +9 & h +7 otherwise pawns will wrap around the board
            // Reverse the logic for black including a and h
            case "Pawn":
            {
                FindPseudoLegalPawnMoves(posInArray, fileAsInt, rank, isWhite);
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
            var currentOffset = Square.Offsets[i];
            // Inner loop goes over each square individually for each offset
            for (var j = 1; j < numUntilEdge + 1; j++)
            {
                //var targetSquare = BoardManager.FindSquareFromBoardPos(posInArray + (currentOffset * j));
                var targetSquare = posInArray + currentOffset * j;

                if (IsSquareOccupied(targetSquare))
                {
                    // The target square contains an enemy piece, it can be captured but the piece can move no further.
                    if (BoardManager.Board[targetSquare].isPieceWhite != isWhite)
                    {
                        _pseudoLegalMoveList.Add(BoardManager.Board[targetSquare]);
                        // TODO: This is probably a good spot to look for pins when adding check logic. 
                    }

                    // The target square contains a friendly piece, it cannot be moved to, or passed.
                    break;
                }

                // If the target square is not occupied, it can be moved to.
                _pseudoLegalMoveList.Add(BoardManager.Board[targetSquare]);
            }
        }
        //return moves;
    }

    private static void FindPseudoLegalKnightMoves(int posInArray, int file, int rank, bool isWhite)
    {
        // Instead of calculating direct offsets I opt to send out "feelers" in each normal direction (Up-Down: +-8, Left-Right: +-1)
        // Then, if those lines are within the proper bounds to move the knight I will extend outward again using the same offsets to find the actual move for the knight.

        var checkUp = true;
        var checkDown = true;
        var checkLeft = true;
        var checkRight = true;

        switch (rank)
        {
            case 6:
            case 7:
                checkUp = false;
                break;
            case 0:
            case 1:
                checkDown = false;
                break;
        }

        switch (file)
        {
            case 0:
            case 1:
                checkLeft = false;
                break;
            case 6:
            case 7:
                checkRight = false;
                break;
        }

        if (checkUp)
            CheckKnightDirection(posInArray, file, 16, 1, isWhite);

        if (checkDown)
            CheckKnightDirection(posInArray, file, -16, 1, isWhite);

        if (checkLeft)
            CheckKnightDirection(posInArray, rank, -2, 8, isWhite);

        if (checkRight)
            CheckKnightDirection(posInArray, rank, 2, 8, isWhite);

        // End FindPseudoLegalKnightMoves
    }

    private static void CheckKnightDirection(int posInArray, int rankOrFile, int initialOffset, int secondOffset,
        bool isWhite)
    {
        var feelerSquarePos = posInArray + initialOffset;

        // Check to the left of the feeler square
        if (rankOrFile != 0)
        {
            if (BoardManager.Board[feelerSquarePos - secondOffset].isOccupied)
            {
                switch (isWhite)
                {
                    case true when !BoardManager.Board[feelerSquarePos - secondOffset].isPieceWhite:
                        _pseudoLegalMoveList.Add(BoardManager.Board[feelerSquarePos - secondOffset]);
                        _movesInList++;
                        break;
                    case false when BoardManager.Board[feelerSquarePos - secondOffset].isPieceWhite:
                        _pseudoLegalMoveList.Add(BoardManager.Board[feelerSquarePos - secondOffset]);
                        _movesInList++;
                        break;
                }
            }
            else
            {
                _pseudoLegalMoveList.Add(BoardManager.Board[feelerSquarePos - secondOffset]);
                _movesInList++;
            }
        }

        if (rankOrFile == 7) return;
        if (BoardManager.Board[feelerSquarePos + secondOffset].isOccupied)
        {
            switch (isWhite)
            {
                case true when !BoardManager.Board[feelerSquarePos + secondOffset].isPieceWhite:
                    _pseudoLegalMoveList.Add(BoardManager.Board[feelerSquarePos + secondOffset]);
                    _movesInList++;
                    break;
                case false when BoardManager.Board[feelerSquarePos + secondOffset].isPieceWhite:
                    _pseudoLegalMoveList.Add(BoardManager.Board[feelerSquarePos + secondOffset]);
                    _movesInList++;
                    break;
            }
        }
        else
        {
            _pseudoLegalMoveList.Add(BoardManager.Board[feelerSquarePos + secondOffset]);
            _movesInList++;
        }
    }

    private static void FindPseudoLegalPawnMoves(int posInArray, int file, int rank, bool isPieceWhite)
    {
        // up down left right will all be in coordinate to piece color here
        // So, for a black piece up will technically be down, it's just in relation to the piece's movement orientation
        var forwardOffset = 0;
        var leftOffset = 0;
        var rightOffset = 0;

        if (!isPieceWhite)
        {
            forwardOffset = -8;
            leftOffset = -7;
            rightOffset = -9;
        }
        else
        {
            forwardOffset = 8;
            leftOffset = 7;
            rightOffset = 9;
        }

        if (!BoardManager.Board[posInArray + forwardOffset].isOccupied)
        {
            _pseudoLegalMoveList.Add(BoardManager.Board[posInArray + forwardOffset]);
            _movesInList++;
        }

        // Still in starting position so the pawns can be double pushed
        if ((rank == 6 && !isPieceWhite) || (rank == 1 && isPieceWhite))
        {
            if (!BoardManager.Board[posInArray + forwardOffset * 2].isOccupied &&
                !BoardManager.Board[posInArray + forwardOffset].isOccupied)
            {
                _pseudoLegalMoveList.Add(BoardManager.Board[posInArray + forwardOffset * 2]);
                _movesInList++;
            }
        }

        if (file != 0 && file != 7)
        {
            // Scenario for black b - g pawns to capture a white piece diagonally
            if (BoardManager.Board[posInArray + leftOffset].isOccupied &&
                IsOpponentPiece(posInArray + leftOffset,
                    isPieceWhite)) //TODO Needs to check opposite color for both not just white
            {
                _pseudoLegalMoveList.Add(BoardManager.Board[posInArray + leftOffset]);
                _movesInList++;
            }

            if (BoardManager.Board[posInArray + rightOffset].isOccupied &&
                IsOpponentPiece(posInArray + rightOffset, isPieceWhite))
            {
                _pseudoLegalMoveList.Add(BoardManager.Board[posInArray + rightOffset]);
                _movesInList++;
            }

            // En passant
            EnPassantCheck(posInArray, file, isPieceWhite);
        }
        else
            switch (file)
            {
                case 0:
                {
                    if (BoardManager.Board[posInArray + leftOffset].isOccupied &&
                        IsOpponentPiece(posInArray + leftOffset, isPieceWhite))
                    {
                        _pseudoLegalMoveList.Add(BoardManager.Board[posInArray + leftOffset]);
                        _movesInList++;
                    }

                    EnPassantCheck(posInArray, file, isPieceWhite);

                    break;
                }
                case 7:
                {
                    if (BoardManager.Board[posInArray + rightOffset].isOccupied &&
                        IsOpponentPiece(posInArray + rightOffset, isPieceWhite))
                    {
                        _pseudoLegalMoveList.Add(BoardManager.Board[posInArray + rightOffset]);
                        _movesInList++;
                    }

                    EnPassantCheck(posInArray, file, isPieceWhite);

                    break;
                }
            }
        // End FindPseudoLegalPawnMoves
    }

    // TODO: Maybe just throw all of this code out and do this instead:
    // Only check for en passent on the 4th and 5th rank, that's the only place they can happen.
    // Then, just check to each square besides the pawn, if it was a double pawn push add en passant.
    // CONNECT TO FEN GENERATION
    private static void EnPassantCheck(int posInArray, int file, bool isPieceWhite)
    {
        if (!isPieceWhite)
        {
            switch (file)
            {
                case > 0 and < 7:
                {
                    if (MoveTracker.ToBoardPos == posInArray + 1 || MoveTracker.ToBoardPos == posInArray - 1)
                    {
                        var offset = MoveTracker.ToBoardPos - posInArray;

                        if (BoardManager.Board[posInArray + offset].pieceOnSquare == "Pawn" &&
                            BoardManager.Board[posInArray + offset].isPieceWhite)
                        {
                            if (MoveTracker.FromBoardPos == MoveTracker.ToBoardPos - 16)
                            {
                                if (!BoardManager.Board[posInArray + offset - 8].isOccupied)
                                {
                                    _pseudoLegalMoveList.Add(BoardManager.Board[posInArray + offset - 8]);
                                    EnPassantMoveNum = _movesInList;
                                    _movesInList++;
                                    CanEnPassant = true;
                                }
                            }
                        }
                    }

                    break;
                }
                case 0:
                {
                    if (MoveTracker.ToBoardPos == posInArray + 1)
                    {
                        const int offset = 1;

                        if (BoardManager.Board[posInArray + offset].pieceOnSquare == "Pawn" &&
                            BoardManager.Board[posInArray + offset].isPieceWhite)
                        {
                            if (MoveTracker.FromBoardPos == MoveTracker.ToBoardPos - 16)
                            {
                                if (!BoardManager.Board[posInArray + offset - 8].isOccupied)
                                {
                                    _pseudoLegalMoveList.Add(BoardManager.Board[posInArray + offset - 8]);
                                    EnPassantMoveNum = _movesInList;
                                    _movesInList++;
                                    CanEnPassant = true;
                                }
                            }
                        }
                    }

                    break;
                }
                case 7:
                {
                    if (MoveTracker.ToBoardPos == posInArray - 1)
                    {
                        const int offset = -8;

                        if (BoardManager.Board[posInArray + offset].pieceOnSquare == "Pawn" &&
                            BoardManager.Board[posInArray + offset].isPieceWhite)
                        {
                            if (MoveTracker.FromBoardPos == MoveTracker.ToBoardPos - 16)
                            {
                                if (!BoardManager.Board[posInArray + offset - 1].isOccupied)
                                {
                                    _pseudoLegalMoveList.Add(BoardManager.Board[posInArray + offset - 8]);
                                    EnPassantMoveNum = _movesInList;
                                    _movesInList++;
                                    CanEnPassant = true;
                                }
                            }
                        }
                    }

                    break;
                }
            }
        }
        else
        {
            switch (file)
            {
                case > 0 and < 7:
                {
                    if (MoveTracker.ToBoardPos == posInArray + 1 || MoveTracker.ToBoardPos == posInArray - 1)
                    {
                        var offset = MoveTracker.ToBoardPos - posInArray;

                        if (BoardManager.Board[posInArray + offset].pieceOnSquare == "Pawn" &&
                            !BoardManager.Board[posInArray + offset].isPieceWhite)
                        {
                            if (MoveTracker.FromBoardPos == MoveTracker.ToBoardPos + 16)
                            {
                                if (!BoardManager.Board[posInArray + offset + 8].isOccupied)
                                {
                                    _pseudoLegalMoveList.Add(BoardManager.Board[posInArray + offset + 8]);
                                    EnPassantMoveNum = _movesInList;
                                    _movesInList++;
                                    CanEnPassant = true;
                                }
                            }
                        }
                    }

                    break;
                }
                case 0:
                {
                    // En Passant
                    if (MoveTracker.ToBoardPos == posInArray + 1)
                    {
                        const int offset = 1;

                        if (BoardManager.Board[posInArray + offset].pieceOnSquare == "Pawn" &&
                            !BoardManager.Board[posInArray + offset].isPieceWhite)
                        {
                            if (MoveTracker.FromBoardPos == MoveTracker.ToBoardPos + 16)
                            {
                                if (!BoardManager.Board[posInArray + offset + 8].isOccupied)
                                {
                                    _pseudoLegalMoveList.Add(BoardManager.Board[posInArray + offset + 8]);
                                    EnPassantMoveNum = _movesInList;
                                    _movesInList++;
                                    CanEnPassant = true;
                                }
                            }
                        }
                    }

                    break;
                }
                case 7:
                {
                    // En Passant
                    if (MoveTracker.ToBoardPos == posInArray - 1)
                    {
                        const int offset = -1;

                        if (BoardManager.Board[posInArray + offset].pieceOnSquare == "Pawn" &&
                            !BoardManager.Board[posInArray + offset].isPieceWhite)
                        {
                            if (MoveTracker.FromBoardPos == MoveTracker.ToBoardPos + 16)
                            {
                                if (!BoardManager.Board[posInArray + offset + 8].isOccupied)
                                {
                                    _pseudoLegalMoveList.Add(BoardManager.Board[posInArray + offset + 8]);
                                    EnPassantMoveNum = _movesInList;
                                    _movesInList++;
                                    CanEnPassant = true;
                                }
                            }
                        }
                    }

                    break;
                }
            }
        }
        // End EnPassantCheck
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
}