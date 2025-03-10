using Core;
using UnityEngine;

public class LocalPlayer : IPlayer
{
    public PieceColor Color { get; private set; }

    // We'll store the last move the user made
    private Move pendingMove;

    public LocalPlayer(PieceColor color)
    {
        Color = color;
    }

    public Move GetMove(GameState gameState)
    {
        // If we have no pending move, we can't return anything valid yet.
        // In practice, game loop can call this in an update cycle or coroutine
        // until a valid move is returned.
        if (pendingMove != null)
        {
            Move result = pendingMove;
            pendingMove = null;
            return result;
        }

        return null;
    }

    // Called by UI or input system once the user selects a valid move
    public void OnLocalUserMoveSelected(Move move)
    {
        // todo: check if it is the players turn
        // todo: maybe validate move here, even though it will likely already be validated.
        pendingMove = move;
    }
}
