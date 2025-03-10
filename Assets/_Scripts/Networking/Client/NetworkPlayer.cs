using Core;
using UnityEngine;

public class NetworkPlayer : IPlayer
{
    public PieceColor Color { get; private set; }

    private Move pendingMove;

    public NetworkPlayer(PieceColor color)
    {
        Color = color;
    }

    public Move GetMove(GameState gameState)
    {
        if (pendingMove != null)
        {
            Move move = pendingMove;
            pendingMove = null;
            return move;
        }
        return null;
    }

    // Called by client (TCPClient.OnMessageReceived, etc.)
    // whenever a "MOVE|" command arrives from the server.
    public void OnNetworkMoveReceived(Move move)
    {
        pendingMove = move;
    }
}
