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
            return;
        }

        Move attemptedMove = new Move(selectedSquare, clickedSquare, "");

        // 2) If we already have a selected piece, try moving it to the clicked square
        // BoardManager.Instance.TryMovePiece(selectedSquare, clickedSquare);
        Player me = GameManager.Instance.GetMyPlayer();
        int result = me.OnMove(attemptedMove);

        // If the result is -1 that means the move was not legal.
        // Calling OnSquareClicked again with the newly last clicked square will reselct that square if it has
        // the correct color piece on it
        if (result == -1)
        {
            UnselectSquare();
            OnSquareClicked(clickedSquare);
        }
        else
        {
            UnselectSquare();
        }
    }

    private void TrySelectSquare(Square clickedSquare)
    {
        // check if it's my turn and piece color matches me
        if (!GameManager.Instance.IsMyTurn() || clickedSquare.Piece.GetColor() != GameManager.Instance.MyColor) { return; }

        // Check if there's a piece on that square
        if (clickedSquare.Piece.GetType() == PieceType.None)
        {
            // No piece to select
            Debug.Log($"TrySelectSquare: No piece on square# {clickedSquare.Index}");
            return;
        }

        selectedSquare = clickedSquare;

        Square[] legalMoves = LegalMovesHandler.FindLegalMoves(GameManager.Instance.GameState, selectedSquare);

        HighlightSquares(legalMoves);
    }

    private void HighlightSquares(Square[] squaresToHighlight)
    {
        foreach (Square sq in squaresToHighlight)
        {
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
