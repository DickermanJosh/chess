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

    [Header("Castling Rights from FEN")] public static bool WKingSideCastle;
    public static bool BKingSideCastle;
    public static bool WQueenSideCastle;
    public static bool BQueenSideCastle;

    [Header("En-Passant Variables")] public static int EnPassantMoveNum;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    private void Start()
    {
        _pseudoLegalMoveList = new List<Square>();
        WKingSideCastle = true;
        BKingSideCastle = true;
        WQueenSideCastle = true;
        BQueenSideCastle = true;
    }

    public static Square[] FindPseudoLegalMoves(string pieceType, bool isWhite, int posInArray, char file, int rank,
        Vector2 pos)
    {
        _pseudoLegalMoveList.Clear();
        _movesInList = 0;
        var fileAsInt = (int)pos.x;
        FENHandler.EnPassantSquare = "-";

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

        if (rank == 7 || rank == 0)
        {
            Debug.Log("promote");
            return;
        }
        // Single push forward
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
        
        // Capture Logic
        switch (file)
        {
            case > 0 and < 7:
            {
                PawnCaptureCheck(posInArray, leftOffset, rank, file, isPieceWhite);
                PawnCaptureCheck(posInArray, rightOffset, rank, file, isPieceWhite);
                break;
            }
            case 0:
            {
                PawnCaptureCheck(posInArray, rightOffset, rank, file, isPieceWhite);
                break;
            }
            case 7:
            {
                PawnCaptureCheck(posInArray, leftOffset, rank, file, isPieceWhite);
                break;
            }
        }
        // End FindPseudoLegalPawnMoves
    }

    private static void PawnCaptureCheck(int posInArray, int directionalOffset, int rank, int file, bool isPieceWhite)
    {
        if (BoardManager.Board[posInArray + directionalOffset].isOccupied &&
            IsOpponentPiece(posInArray + directionalOffset, isPieceWhite))
        {
            _pseudoLegalMoveList.Add(BoardManager.Board[posInArray + directionalOffset]);
            _movesInList++;
        }

        if ((isPieceWhite && rank == 4) || (!isPieceWhite && rank == 3))
            EnPassantCheck(posInArray, file, isPieceWhite);
    }

    private static void EnPassantCheck(int posInArray, int file, bool isPieceWhite)
    {
        var forwardOffset = 0;
        var leftOffset = 0;
        var rightOffset = 0;
        if (!isPieceWhite)
        {
            forwardOffset = -8;
            leftOffset = 1;
            rightOffset = -1;
        }
        else
        {
            forwardOffset = 8;
            leftOffset = -1;
            rightOffset = 1;
        }

        switch (file)
        {
            case > 0 and < 7:
            {
                HandleEnPassantCheck(posInArray, leftOffset, forwardOffset);
                HandleEnPassantCheck(posInArray, rightOffset, forwardOffset);
                break;
            }
            case 0:
            {
                // R for White -- L for Black
                HandleEnPassantCheck(posInArray, isPieceWhite ? rightOffset : leftOffset, forwardOffset);
                break;
            }
            case 7:
            {
                HandleEnPassantCheck(posInArray, isPieceWhite ? leftOffset : rightOffset, forwardOffset);
                break;
            }
        }
        // End EnPassantCheck
    }

    private static void HandleEnPassantCheck(int posInArray, int directionalOffset, int upwardOffset)
    {
        // Checking if the last move was a pawn pushed to the left of our current pawn.
        if (MoveTracker.ToBoardPos != posInArray + directionalOffset ||
            BoardManager.Board[posInArray + directionalOffset].pieceOnSquare != "Pawn") return;
        if (!WasTheLastMoveADoublePawnPush()) return;

        EnPassantMoveNum = posInArray + directionalOffset + upwardOffset;
        if (BoardManager.Board[EnPassantMoveNum].isOccupied) return;

        _pseudoLegalMoveList.Add(BoardManager.Board[EnPassantMoveNum]);

        // Gathering and sending the results to the FEN Handler
        var targetSquare = BoardManager.Board[EnPassantMoveNum];
        var targetFile = char.ToString(targetSquare.file);
        var targetRank = targetSquare.rank + 1;
        FENHandler.EnPassantSquare = targetFile + targetRank;
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

    private static bool WasTheLastMoveADoublePawnPush()
    {
        var from = MoveTracker.FromBoardPos;
        var to = MoveTracker.ToBoardPos;
        var toSquare = BoardManager.Board[to];
        if (toSquare.pieceOnSquare != "Pawn") return false;
        return (toSquare.isPieceWhite && from == to - 16) || (!toSquare.isPieceWhite && from == to + 16);
    }
}