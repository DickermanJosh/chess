using Core;
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

    }
    private static void CheckWelcomeReceived(string message)
    {
       if (message.Equals("WELCOME"))
        {
            
        }
    }

    private static void CheckQueueOkReceived(string message)
    {
        if (message.Equals("QUEUEOK"))
        {
            // TODO: Start a client side queue clock and switch the queue button to a dequeue button
        }
    }

    /// <summary>
    /// MATCH_START is sent from the server when two players enter a match together.
    /// Once the client receives this message a client-side match is started and the player is
    /// loaded into the game scene
    /// </summary>
    /// <param name="message"></param>
    private static void CheckMatchStartedReceived(string message)
    {
        if (message.StartsWith("MATCH_START|"))
        {
            Log($"Match start hit: raw string: {message}");
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
    }

    #endregion

    #region Messages To Send

    /// <summary>
    /// Returns the CONNECT_ID message with the stored player identity
    /// </summary>
    public static string GetConnectId()
    {
        return $"CONNECT_ID|{PlayerIdentity.PlayerId}|{PlayerIdentity.PlayerName}";
    }
    /// <summary>
    /// Returns the QUEUEUP message with the stored player identity
    /// </summary>
    public static string GetQueueUp()
    {
        return $"QUEUEUP|{PlayerIdentity.PlayerId}|{PlayerIdentity.PlayerName}";
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
