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

    // Called by client (TCPClient.OnMessageReceived, etc.)
    // whenever a "MOVE|" command arrives from the server.
    public void OnMove(Move move)
    {
        pendingMove = move;
    }
}