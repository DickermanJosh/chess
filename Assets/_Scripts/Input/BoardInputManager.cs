using UnityEngine;
using Core;
using System.Collections.Generic;
using Managers;
using Render;

public class BoardInputManager : MonoBehaviour
{
    public static BoardInputManager Instance { get; private set; }

    // Keep track of the currently selected square/piece
    private Square selectedSquare;

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);

        Instance = this;
    }

    public void OnSquareClicked(Square clickedSquare)
    {
        // 1) If no piece is currently selected, try selecting the piece on that square
        if (selectedSquare == null)
        {
            TrySelectSquare(clickedSquare);
        }
        else
        {
            // 2) If we already have a selected piece, try moving it to the clicked square
            BoardManager.Instance.TryMovePiece(selectedSquare, clickedSquare);

            // Then we can unselect
            UnselectSquare();
        }
    }

    private void TrySelectSquare(Square clickedSquare)
    {
        // Check if there's a piece on that square
        if (clickedSquare.Piece.GetType() == PieceType.None)
        {
            // No piece to select
            return;
        }

        // check if it's my turn and piece color matches me
        if (!GameManager.Instance.IsMyTurn() || clickedSquare.Piece.GetColor() != GameManager.Instance.MyColor) return;

        selectedSquare = clickedSquare;
        // For now, highlight all squares as “legal”
        //HighlightAllSquares();
        Square[] legalMoves = LegalMovesHandler.FindPseudoLegalMoves(selectedSquare);
        HighlightSquares(legalMoves);
    }

    private void HighlightSquares(Square[] squaresToHighlight)
    {
        foreach (Square sq in squaresToHighlight)
        {
            sq.Renderer.AddHighlight();
        }
    }
    private void HighlightAllSquares()
    {
        // Temporary approach: highlight everything
        var board = GameManager.Instance.GameState.Board;
        foreach (Square sq in board.squares)
        {
            // SquareHighlightManager.Instance.HighlightSquare(sq, Color.yellow);
            sq.Renderer.AddHighlight();
        }
    }

    public void UnselectSquare()
    {
        selectedSquare = null;
        // SquareHighlightManager.Instance.ClearAllHighlights();
        var board = GameManager.Instance.GameState.Board;
        foreach (Square sq in board.squares)
        {
            sq.Renderer.RemoveHighlight();
        }
    }
}
