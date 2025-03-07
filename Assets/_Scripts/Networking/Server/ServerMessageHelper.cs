using UnityEngine;

public static class ServerMessageHelper
{
    #region Check Received Messages

    public static void HandleReceivedMessage(RemotePlayerConnection conn, string message)
    {
        CheckConnectionReceived(conn, message);
        CheckQueueReceived(conn, message);
        CheckMoveReceived(conn, message);
    }
    private static void CheckConnectionReceived(RemotePlayerConnection conn, string message)
    {
        if (message.StartsWith("CONNECT_ID|"))
        {
            // Example: "CONNECT_ID|myGuid|CoolChessDude"
            string[] parts = message.Split('|');
            // parts[0] = "CONNECT_ID"
            // parts[1] = <player_id>
            // parts[2] = <player_name>

            string clientId = parts[1];
            string clientName = parts[2];

            // Store these in the RemotePlayerConnection (if you trust the client's ID)
            conn.PlayerId = clientId;
            conn.PlayerName = clientName;

            Debug.Log($"[Server] Client {conn.PlayerName} established with ID | {conn.PlayerId}");

            // Optionally, send back an acknowledgment
            conn.Send("WELCOME");
        }
    }
    private static void CheckQueueReceived(RemotePlayerConnection conn, string message)
    {
        if (message.StartsWith("QUEUEUP|"))
        {
            var parts = message.Split('|');
            // parts[0] = "QUEUEUP"
            // parts[1] = <player_id>
            // parts[2] = <player_name>

            // string playerId = parts[1];
            // string playerName = parts[2];

            // Update conn's known name + ID if needed
            // conn.PlayerName = playerName;
            // conn.PlayerId is typically set at creation,
            // but if you're reading ID from the message, you can store it.

            MatchmakingManager.EnqueuePlayer(conn);

        }
    }

    private static void CheckMoveReceived(RemotePlayerConnection conn, string message) 
    {
        if (message.StartsWith("MOVE|"))
        {
            // parse move, find the match, etc.
        }
    }


    #endregion

    #region Messages To Send

    #endregion

    #region Console Logging

    public static void Log(string message)
    {
        Debug.Log($"[Server] {message}");
    }

    public static void LogError(string message)
    {
        Debug.LogError($"[Server] {message}");
    }

    #endregion
}
