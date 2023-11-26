using System;
using System.Linq;
using UnityEngine;
public class BoardManager : MonoBehaviour
{
    private static BoardManager _instance;
    public static BoardManager Instance => _instance;

    public static readonly Square[] Board = new Square[64];
    private static Square[] _legalMoves = new Square[64];
    private static readonly int[] HighlightedPositions = new int[64];
    public static int[][] NumSquaresToEdge;
    
    public static Square SelectedSquare;
    private static int _amountOfHighlights = 0;
    
    private static string _to;
    private static string _from;
    private static int _oldPos;
    private static int _newPos;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        SelectedSquare = null;

        //WhitePromotionImage = gameObject.GetComponent<Image>();
    }

    // Function to handle selecting a square in order to move a piece
    public static void SelectSquare(int squarePosInBoard)
    {
        var square = Board[squarePosInBoard];

        // Attempting to move a selected piece to a legal square
        if (SelectedSquare != null && _legalMoves.Contains(square))
        {
            MovePiece(square, square.pieceOnSquare, square.isPieceWhite);

            // Clearing highlighted squares after the piece has moved
            ClearHighLightedSquares();

            Array.Clear(HighlightedPositions, 0, HighlightedPositions.Length);
            _amountOfHighlights = 0;

            //Debug.Log(FENHandler.GetCurrentFenPos());
            return;
        }
        // Attempting to move a selected piece to an illegal square
        if (SelectedSquare != null && !_legalMoves.Contains(square))
        {
            SelectedSquare = null;
            ClearHighLightedSquares();
            return;
        }
 
        _legalMoves = LegalMovesHandler.FindPseudoLegalMoves(square.pieceOnSquare, square.isPieceWhite,
            square.BoardPosInArray, square.file, square.rank, square.pos);
        HighLightAvailableSquares(square, squarePosInBoard);
        SelectedSquare = square;
    }

    // Function to move pieces to new squares, will update FEN, Move order, internal and visual representation of the board
    private static void MovePiece(Square squareToMove, string pieceType, bool isWhite)
    {
        pieceType = SelectedSquare.pieceOnSquare;
        isWhite = SelectedSquare.isPieceWhite;

        if (squareToMove.isOccupied)
        {
            squareToMove.RemovePieceFromSquare();
        }

        if (FENHandler.EnPassantSquare != "-")
        {
            Debug.Log(FENHandler.EnPassantSquare);
            if (squareToMove.BoardPosInArray == LegalMovesHandler.EnPassantMoveNum)
            {
                if (!isWhite)
                {
                    var index = squareToMove.BoardPosInArray + 8;
                    Board[index].RemovePieceFromSquare();
                }
                else
                {
                    var index = squareToMove.BoardPosInArray - 8;
                    Board[index].RemovePieceFromSquare();
                }
            }
        }

        Board[squareToMove.BoardPosInArray].AddPieceToSquare(pieceType, isWhite, UIManager.isFlipped);
        Board[SelectedSquare.BoardPosInArray].RemovePieceFromSquare();

        var from = SelectedSquare.file + SelectedSquare.rank.ToString();
        var to = squareToMove.file + squareToMove.rank.ToString();


        // Promotion Handling
        if (squareToMove.rank is 0 or 7 && pieceType == "Pawn")
        {
            PawnPromotionHandler.Instance.promotedSquare = squareToMove.BoardPosInArray;
            PawnPromotionHandler.Instance.OnPromotionCompleted += HandlePromotionCompletion;
            PawnPromotionHandler.Instance.TogglePromotionTable(isWhite);
            
            _to = to;
            _from = from;
            _oldPos = squareToMove.BoardPosInArray;
            _newPos = SelectedSquare.BoardPosInArray;
            
        }
        else
        {
            UpdateMoveOrder(to, from, squareToMove.BoardPosInArray,
                SelectedSquare.BoardPosInArray);
            //MoveTracker.Instance.Move(to, from, squareToMove.BoardPosInArray,
            //    SelectedSquare.BoardPosInArray); // Updating the move order
        }

        SelectedSquare = null;
        _legalMoves = null;

        FENHandler.GetCurrentFenPos();
    }
    
    private static void HandlePromotionCompletion()
    {
        UpdateMoveOrder(_to, _from, _oldPos, _newPos);

        // Unsubscribe to prevent memory leaks
        PawnPromotionHandler.Instance.OnPromotionCompleted -= HandlePromotionCompletion;
    }

    // Called only in the SelectSquares function
    // This function will highlight all available squares that a selected piece can move to based on its legal moves
    private static void HighLightAvailableSquares(Square square, int squarePosInBoard)
    {
        if (SelectedSquare != null && !_legalMoves.Contains(square))
        {
            foreach (var sq in HighlightedPositions)
                Board[sq].HighLightSquare(false);

            SelectedSquare = null;
            return;
        }

        square.HighLightSquare(true);
        HighlightedPositions[_amountOfHighlights] = squarePosInBoard;
        _amountOfHighlights++;

        if (_legalMoves.Length <= 0) return;
        foreach (var t in _legalMoves)
        {
            if (t == null) continue;
            var pos = t.BoardPosInArray;
            Board[pos].HighLightSquare(true);
            HighlightedPositions[_amountOfHighlights] = pos;
            _amountOfHighlights++;
        }
    }

    private static void ClearHighLightedSquares()
    {
        // Clearing highlighted squares after the piece has moved
        foreach (var sq in HighlightedPositions)
            Board[sq].HighLightSquare(false);
    }
    
    private static void UpdateMoveOrder(string to, string from, int newPosition, int oldPosition)
    {
        MoveTracker.Instance.Move(to, from, newPosition, oldPosition);
    }

    // Utility function to find the position of a square in BoardManager's Board array based off its rank and file positions
    public static int SquarePosInArrayFromVectorPos(Vector2 pos) // pos.x = file, pos.y = rank
    {
        if (pos is { x: 0, y: 0 })
            return 0;

        var count = 0;
        for (var rank = 0; rank < 8; rank++)
        {
            for (var file = 0; file < 8; file++)
            {
                if (Math.Abs(pos.x - file) < 0.1f && Math.Abs(pos.y - rank) < 0.1f)
                {
                    return count;
                }

                count++;
            }
        }

        return 999;
    }

    public static Square FindSquareFromBoardPos(int posInBoard)
    {
        return Board.FirstOrDefault(sq => sq.BoardPosInArray == posInBoard);
    }
    
    // Utility function used to remove a piece from the board either when it is moving to a new square or being captured
    public static void ClearPiecesFromBoard()
    {
        foreach (var square in Board)
        {
            square.RemovePieceFromSquare();
        }
    }
}