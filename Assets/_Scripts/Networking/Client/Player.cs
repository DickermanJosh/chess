using Core;
using UnityEngine;

public class Player
{
    public PieceColor Color { get; private set; }

    public Player(PieceColor color)
    {
        Color = color;
    }

    /// <summary>
    /// Called by the input system once two squares have been selected
    /// Performs local move validation and sends results to the server if they are valid
    /// </summary>
    /// <returns> 0 - Move sent to server. -1 - Move not legal. -2 - Wrong color to move or piece color selected </returns>
    public int OnMove(Move move)
    {
        // todo: check if it is the players turn
        // todo: maybe validate move here, even though it will likely already be validated.
        Square toSquare = move.To;
        Square fromSquare = move.From;

        // Check to make sure that:
        // 1. The squares selected are not the same square
        // 2. It is the correct players turn
        // 3. The piece being moved is the right color
        if (toSquare == fromSquare ||
            GameManager.Instance.MyColor != GameManager.Instance.GameState.ColorToMove ||
            GameManager.Instance.MyColor != fromSquare.Piece.GetColor())
        {
            Debug.Log("[BoardManager TryMovePiece()] Check failed. See Breakdown.");
            Debug.Log($"FromSquare: {fromSquare.Index} - {fromSquare.Piece.GetType()}");
            Debug.Log($"ToSquare: {toSquare.Index} - {toSquare.Piece.GetType()}");
            Debug.Log($"MyColor: {GameManager.Instance.MyColor} MoveColor: {GameManager.Instance.GameState.ColorToMove}");
            Debug.Log($"Piece Color: {fromSquare.Piece.GetColor()}");
            return -2;
        }

        //  1. Perform Local move validation to confirm legal move
        // GameState thisClientGS = GameManager.Instance.GameState;

        if (!LegalMovesHandler.IsMoveLegal(GameManager.Instance.GameState, toSquare, fromSquare))
        {
            Debug.Log("[BoardManager TryMovePiece()] Move Not legal.");
            // GameManager.Instance.GameState = thisClientGS;
            return -1;
        }

        Debug.Log($"[BoardManager] TryMovePiece() Sending [{fromSquare.Coord}->{toSquare.Coord}]");
        // 2. Send the move to the server e.g "MOVE|e2|e4"
        ClientMessageHelper.SendMove(fromSquare, toSquare);

        // TODO: Make this authoritative later somehow
        GameManager.Instance.GameState.MoveTracker.AddMove(move);

        // End move client side to prevent sending multiple messages
        if (GameManager.Instance.MyColor != PieceColor.White)
        {
            GameManager.Instance.GameState.ColorToMove = PieceColor.White;
        }
        else
        {
            GameManager.Instance.GameState.ColorToMove = PieceColor.Black;
        }

        return 0;
    }
}
