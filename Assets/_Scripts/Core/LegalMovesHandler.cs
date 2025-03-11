using Core;
using Managers;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for generating pseudo-legal moves for pieces. 
/// Contains logic for sliding moves, knight moves, pawn moves, etc.
/// The script references GameManager for board state.
/// </summary>
public class LegalMovesHandler : MonoBehaviour
{
    [Header("Singleton Declaration")]
    private static LegalMovesHandler _instance;
    public static LegalMovesHandler Instance => _instance;

    // We store temporary pseudo-legal moves here.
    private static List<Square> _pseudoLegalMoveList;
    private static int _movesInList;

    [Header("Castling Rights from FEN")]
    public static bool WKingSideCastle;
    public static bool BKingSideCastle;
    public static bool WQueenSideCastle;
    public static bool BQueenSideCastle;

    [Header("En-Passant Variables")]
    public static int EnPassantMoveNum;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    private void Start()
    {
        _pseudoLegalMoveList = new List<Square>();

        // Default castling rights (can be updated from FEN)
        WKingSideCastle = true;
        BKingSideCastle = true;
        WQueenSideCastle = true;
        BQueenSideCastle = true;
    }

    /// <summary>
    /// Main entry for generating pseudo-legal moves for the piece on Square sq.
    /// Currently incomplete for King, etc.
    /// </summary>
    public static Square[] FindPseudoLegalMoves(Square sq)
    {
        // Clear from previous usage
        _pseudoLegalMoveList.Clear();
        _movesInList = 0;

        // Reset or read En Passant data
        FENUtils.EnPassantSquare = "-";

        int posInArray = sq.Index;
        PieceType pieceType = sq.Piece.GetType();
        // PieceColor color = sq.Piece.GetColor();
        PieceColor color = GameManager.Instance.MyColor;

        switch (pieceType)
        {
            case PieceType.King:
                // TODO: handle King logic or castling properly
                // FindPseudoLegalKingMoves(...);
                break;

            case PieceType.Queen:
            case PieceType.Rook:
            case PieceType.Bishop:
                FindSlidingPieceMoves(posInArray, pieceType, color);
                break;

            case PieceType.Knight:
                {
                    // We need file, rank. For example:
                    int file = sq.Coord.file;
                    int rank = sq.Coord.rank;
                    FindPseudoLegalKnightMoves(posInArray, file, rank, color);
                    break;
                }

            case PieceType.Pawn:
                {
                    int file = sq.Coord.file;
                    int rank = sq.Coord.rank;
                    FindPseudoLegalPawnMoves(posInArray, file, rank, color);
                    break;
                }

            case PieceType.None:
                // No piece => no moves
                break;
        }

        Debug.Log($"[LegalMovesHandler] Found {_movesInList} pseudo-legal moves for {pieceType} at index {posInArray}.");
        return _pseudoLegalMoveList.ToArray();
    }

    /// <summary>
    /// Finds sliding moves (Bishop, Rook, Queen). Uses Board.Offsets[] 
    /// and each piece's limited directions.
    /// </summary>
    private static void FindSlidingPieceMoves(int posInArray, PieceType pieceType, PieceColor color)
    {
        Square startSquare = Board.GetSquareFromIndex(posInArray);

        // For offset usage:
        int startIndex = 0;
        int endIndex = 8; // 0..7 = all directions

        switch (pieceType)
        {
            case PieceType.Rook:
                // Rooks only use first 4 directions (N,S,E,W)
                endIndex = 4;
                break;

            case PieceType.Bishop:
                // Bishops only use last 4 directions (diagonals)
                startIndex = 4;
                break;

                // Queen uses all, so no changes
        }

        // Go through each direction
        for (int i = startIndex; i < endIndex; i++)
        {
            int numUntilEdge = startSquare.AvailableDistancesToEdge[i];
            int currentOffset = Board.Offsets[i];

            // Move along that direction step by step
            for (int j = 1; j <= numUntilEdge; j++)
            {
                int targetIndex = posInArray + currentOffset * j;

                if (IsSquareOccupied(targetIndex))
                {
                    // Occupied => can capture if it's an opponent
                    if (GetPieceColor(targetIndex) != color)
                    {
                        AddMove(targetIndex);
                    }
                    // Stop searching further in this direction
                    break;
                }
                // Not occupied => can move here, keep going
                AddMove(targetIndex);
            }
        }
    }

    /// <summary>
    /// Example stub for King moves.
    /// (Currently not implemented fully.)
    /// </summary>
    private static void FindPseudoLegalKingMoves(int posInArray, int fileAsInt, int rank, PieceColor color)
    {
        // TODO: implement King step logic, castling checks, etc.
        // For example, offsets = new int[] { +8, -8, +1, -1, +9, -9, +7, -7 };
    }

    /// <summary>
    /// Knight moves. We do a partial approach with "feeler squares."
    /// </summary>
    private static void FindPseudoLegalKnightMoves(int posInArray, int file, int rank, PieceColor color)
    {
        bool checkUp = !(rank == 6 || rank == 7);
        bool checkDown = !(rank == 0 || rank == 1);
        bool checkLeft = !(file == 0 || file == 1);
        bool checkRight = !(file == 6 || file == 7);

        if (checkUp)
            CheckKnightDirection(posInArray, file, 16, 1, color);

        if (checkDown)
            CheckKnightDirection(posInArray, file, -16, 1, color);

        if (checkLeft)
            CheckKnightDirection(posInArray, rank, -2, 8, color);

        if (checkRight)
            CheckKnightDirection(posInArray, rank, 2, 8, color);
    }

    /// <summary>
    /// For the Knight, we do an initial offset (up/down 2 squares?), 
    /// then we check left/right squares from there, etc.
    /// </summary>
    private static void CheckKnightDirection(int posInArray, int rankOrFile, int initialOffset, int secondOffset, PieceColor color)
    {
        int feelerSquarePos = posInArray + initialOffset;

        // "Left" direction from that feeler
        if (rankOrFile != 0)
        {
            int targetIndex = feelerSquarePos - secondOffset;
            if (IsSquareValid(targetIndex))
            {
                if (IsSquareOccupied(targetIndex))
                {
                    // Occupied => can capture if enemy
                    if (GetPieceColor(targetIndex) != color)
                        AddMove(targetIndex);
                }
                else
                {
                    AddMove(targetIndex);
                }
            }
        }

        // "Right" direction from that feeler
        if (rankOrFile != 7)
        {
            int targetIndex = feelerSquarePos + secondOffset;
            if (IsSquareValid(targetIndex))
            {
                if (IsSquareOccupied(targetIndex))
                {
                    // Occupied => capture if enemy
                    if (GetPieceColor(targetIndex) != color)
                        AddMove(targetIndex);
                }
                else
                {
                    AddMove(targetIndex);
                }
            }
        }
    }

    /// <summary>
    /// Pawn moves for White/Black. 
    /// Single or double push, captures, etc.
    /// </summary>
    private static void FindPseudoLegalPawnMoves(int posInArray, int file, int rank, PieceColor color)
    {
        // If black, we invert offsets
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
        if (IsSquareValid(forwardIndex) && !IsSquareOccupied(forwardIndex))
        {
            AddMove(forwardIndex);
        }

        // Double push from starting rank
        bool atStartingRank = (color == PieceColor.White && rank == 1) || (color == PieceColor.Black && rank == 6);
        if (atStartingRank)
        {
            int doublePushIndex = posInArray + (forwardOffset * 2);
            if (IsSquareValid(doublePushIndex) &&
                !IsSquareOccupied(doublePushIndex) &&
                !IsSquareOccupied(forwardIndex))
            {
                AddMove(doublePushIndex);
            }
        }

        // Captures
        switch (file)
        {
            case > 0 and < 7:
                PawnCaptureCheck(posInArray, leftOffset, rank, file, color);
                PawnCaptureCheck(posInArray, rightOffset, rank, file, color);
                break;
            case 0:
                PawnCaptureCheck(posInArray, rightOffset, rank, file, color);
                break;
            case 7:
                PawnCaptureCheck(posInArray, leftOffset, rank, file, color);
                break;
        }
    }

    private static void PawnCaptureCheck(int posInArray, int directionalOffset, int rank, int file, PieceColor color)
    {
        int targetIndex = posInArray + directionalOffset;
        if (!IsSquareValid(targetIndex)) return;

        if (IsSquareOccupied(targetIndex) && GetPieceColor(targetIndex) != color)
        {
            AddMove(targetIndex);
        }

        // Possibly do en passant
        if ((color == PieceColor.White && rank == 4) || (color == PieceColor.Black && rank == 3))
        {
            EnPassantCheck(posInArray, file, color);
        }
    }

    private static void EnPassantCheck(int posInArray, int file, PieceColor color)
    {
        int forwardOffset = (color == PieceColor.Black) ? -8 : 8;
        int leftOffset = (color == PieceColor.Black) ? +1 : -1;
        int rightOffset = (color == PieceColor.Black) ? -1 : +1;

        // In effect, "leftOffset" or "rightOffset" are how we find the adjacent file
        switch (file)
        {
            case > 0 and < 7:
                HandleEnPassantCheck(posInArray, leftOffset, forwardOffset);
                HandleEnPassantCheck(posInArray, rightOffset, forwardOffset);
                break;
            case 0:
                HandleEnPassantCheck(posInArray, (color == PieceColor.White) ? rightOffset : leftOffset, forwardOffset);
                break;
            case 7:
                HandleEnPassantCheck(posInArray, (color == PieceColor.White) ? leftOffset : rightOffset, forwardOffset);
                break;
        }
    }

    private static void HandleEnPassantCheck(int posInArray, int fileOffset, int forwardOffset)
    {
        int neighborIndex = posInArray + fileOffset;
        if (!IsSquareValid(neighborIndex)) return;

        // Must be a pawn that just moved there
        var lastMove = MoveTracker.Instance.GetLastMove();
        if (lastMove == null) return; // no moves yet
        if (lastMove.To == null) return;

        if (lastMove.To.Index != neighborIndex) return;
        if (GetPieceType(neighborIndex) != PieceType.Pawn) return;
        if (!WasTheLastMoveADoublePawnPush()) return;

        // The square behind that neighbor => capturing it en passant
        int enPassantIndex = neighborIndex + forwardOffset;
        if (!IsSquareValid(enPassantIndex)) return;

        if (!IsSquareOccupied(enPassantIndex))
        {
            _pseudoLegalMoveList.Add(Board.squares[enPassantIndex]);
            _movesInList++;
            EnPassantMoveNum = enPassantIndex;
            FENUtils.EnPassantSquare = Board.squares[enPassantIndex].Coord.ToString();
        }
    }

    /// <summary>
    /// Checks if the piece is allowed to move (matching the current color to move).
    /// </summary>
    public static bool IsPiecesTurn(bool isWhitePiece)
    {
        // Example: if White's turn, we need isWhitePiece = true, else false
        PieceColor colorToMove = GameManager.Instance.GameState.ColorToMove;
        if (isWhitePiece && colorToMove == PieceColor.White) return true;
        if (!isWhitePiece && colorToMove == PieceColor.Black) return true;
        return false;
    }

    /// <summary>
    /// We consider a square 'occupied' if there is any piece != None.
    /// (Corrected logic from the original version.)
    /// </summary>
    private static bool IsSquareOccupied(int position)
    {
        // If pieceType is not None, it's occupied.
        return GetPieceType(position) != PieceType.None;
    }

    /// <summary>
    /// Checks if position is within [0..63].
    /// </summary>
    private static bool IsSquareValid(int position)
    {
        return (position >= 0 && position < 64);
    }

    /// <summary>
    /// Checks if the piece at 'to' square is an opponent piece.
    /// (We do not check for emptiness here; you can combine with IsSquareOccupied.)
    /// </summary>
    private static bool IsOpponentPiece(int position, PieceColor color)
    {
        return GetPieceType(position) != PieceType.None && (GetPieceColor(position) != color);
    }

    /// <summary>
    /// Checks if the last move was a double-pawn-push (2 squares).
    /// </summary>
    private static bool WasTheLastMoveADoublePawnPush()
    {
        var lastMove = MoveTracker.Instance.GetLastMove();
        if (lastMove == null) return false;

        int from = lastMove.From.Index;
        int to = lastMove.To.Index;

        Square toSquare = Board.squares[to];
        if (toSquare.Piece.GetType() != PieceType.Pawn)
            return false;

        // White double push => from == to - 16
        // Black double push => from == to + 16
        if (toSquare.Piece.GetColor() == PieceColor.White && (from == to - 16)) return true;
        if (toSquare.Piece.GetColor() == PieceColor.Black && (from == to + 16)) return true;

        return false;
    }

    // ------------------------------
    // HELPER METHODS to reduce repeated "GameManager.Instance.GameState.Board" calls
    // ------------------------------

    /// <summary>
    /// A quick reference to the Board from the current GameState.
    /// </summary>
    private static Board Board
    {
        get { return GameManager.Instance.GameState.Board; }
    }

    /// <summary>
    /// Returns the PieceType at a given board index (0..63).
    /// </summary>
    private static PieceType GetPieceType(int index)
    {
        return Board.squares[index].Piece.GetType();
    }

    /// <summary>
    /// Returns the PieceColor at a given board index (0..63).
    /// </summary>
    private static PieceColor GetPieceColor(int index)
    {
        return Board.squares[index].Piece.GetColor();
    }

    /// <summary>
    /// Adds the square at targetIndex to the pseudo-legal move list if valid,
    /// and increments _movesInList.
    /// </summary>
    private static void AddMove(int targetIndex)
    {
        _pseudoLegalMoveList.Add(Board.squares[targetIndex]);
        _movesInList++;
    }
}
