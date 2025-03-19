using Core;
using Managers;
using UnityEngine;

/// <summary>
/// Messages that can be sent from a client to the server
/// </summary>
public static class ClientMessageHelper
{
    #region Messages Received

    public static void HandleReceivedMessage(string message)
    {
        CheckWelcomeReceived(message);
        CheckQueueOkReceived(message);
        CheckMatchStartedReceived(message);
        CheckFenReceived(message);
        CheckMatchEndReceived(message);
    }
    private static void CheckWelcomeReceived(string message)
    {
       if (!message.Equals("WELCOME")) { return; }
    }

    private static void CheckQueueOkReceived(string message)
    {
        if (!message.Equals("QUEUEOK")) { return; }
        // TODO: Start a client side queue clock and switch the queue button to a dequeue button
    }
    private static void CheckFenReceived(string message)
    {
        if (!message.StartsWith("FEN|")) { return; }

        Log("Entered FEN received.");

        string[] t = message.Split("|");
        string fen = t[1];
        string fromString = t[2];
        string toString = t[3];
        Log($"Extracted FEN: {fen}");

        Square from = GameManager.Instance.GameState.Board.GetSquareFromNotation(fromString);
        Square to = GameManager.Instance.GameState.Board.GetSquareFromNotation(toString);
        PieceColor col = (GameManager.Instance?.MyColor == PieceColor.White) ? PieceColor.Black : PieceColor.White;
        Move move = new Move(from, to, col);

        UnityMainThreadDispatcher.Enqueue(() => 
        {
            GameManager.Instance.GameState.MoveTracker.AddMove(move);
            GameManager.Instance.UpdateGameStateFromFen(fen);
        });
    }

    /// <summary>
    /// MATCH_START is sent from the server when two players enter a match together.
    /// Once the client receives this message a client-side match is started and the player is
    /// loaded into the game scene
    /// </summary>
    /// <param name="message"></param>
    private static void CheckMatchStartedReceived(string message)
    {
        if (!message.StartsWith("MATCH_START|")) { return; }

        string[] m = message.Split('|');
        string colorStr = m[1];
        string opponent = m[2];
        PieceColor color = PieceColor.None;

        if (colorStr.Equals("WHITE"))
        {
            color = PieceColor.White;
        }
        else
        {
            color = PieceColor.Black;
        }

        if (color == PieceColor.None)
        {
            Debug.Log($"[Client] Could not interpret piece color assigned by server. Received: {colorStr}");
            return;
        }

        // Start online match with local player and network player
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            GameManager.Instance.OpponentName = opponent;
            GameManager.Instance.StartOnlineGame(color);

            SceneLoader.Instance.LoadScene(SceneLoader.OnlineGame);
        });
    }

    private static void CheckMatchEndReceived(string message)
    {
        if (!message.StartsWith("MATCH_END")) { return; }

        // TODO: message will eventually look something like MATCH_END|checkmate|winner_name
        // Provide a pop up panel with end of match info when that is the case
        // for now, just return to lobby

        UnityMainThreadDispatcher.Enqueue(() => 
        {
            GameManager.Instance.ResetToDefault();
            SceneLoader.Instance.LoadScene(SceneLoader.Lobby);
        });
    }

    #endregion

    #region Messages To Send

    /// <summary>
    /// Sends the CONNECT_ID message to the server with the stored player identity
    /// </summary>
    public static void SendConnectId()
    {
        string message = $"CONNECT_ID|{PlayerIdentity.PlayerId}|{PlayerIdentity.PlayerName}";
        PlayerIdentity.Client.SendMessage(message);
    }
    /// <summary>
    /// Sends the QUEUEUP message to the server with the stored player identity
    /// </summary>
    public static void SendQueueUp()
    {
        string message = $"QUEUEUP|{PlayerIdentity.PlayerId}|{PlayerIdentity.PlayerName}";
        PlayerIdentity.Client.SendMessage(message);
    }

    /// <summary>
    /// Sends the MOVE message to the server with the players attempted move
    /// </summary>
    public static void SendMove(Square from, Square to)
    {
        string message = $"MOVE|{from.Coord}|{to.Coord}";
        PlayerIdentity.Client.SendMessage(message);
    }

    /// <summary>
    /// Sends the RESIGN message to the server, prompting the players current match to end.
    /// Keeps online connection, just ends current match
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public static void SendResign()
    {
        const string message = "RESIGN";
        PlayerIdentity.Client.SendMessage(message);
    }

    #endregion

    #region Logging
    
    public static void Log(string message)
    {
        Debug.Log($"[Client] {message}");
    }

    public static void LogError(string message)
    {
        Debug.LogError($"[Client] {message}");
    }

    #endregion
}
