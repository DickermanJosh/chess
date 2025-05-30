using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class MatchmakingManager
{
    private static Queue<RemotePlayerConnection> waitingQueue = new Queue<RemotePlayerConnection>();
    public static List<Match> activeMatches = new List<Match>();

    public static void EnqueuePlayer(RemotePlayerConnection player)
    {
        if (player.IsInMatch)
        {
            ServerMessageHelper.Log($"{player.PlayerName} is already in a match, ignoring queue request.");
            return;
        }

        waitingQueue.Enqueue(player);
        ServerMessageHelper.Log($"{player.PlayerName} queued. Queue size={waitingQueue.Count}");

        // Acknowledge to the player
        player.Send("QUEUEOK");

        // Check if a match can be formed
        if (waitingQueue.Count >= 2)
        {
            var p1 = waitingQueue.Dequeue();
            var p2 = waitingQueue.Dequeue();

            p1.IsInMatch = true;
            p2.IsInMatch = true;

            CreateMatch(p1, p2);
        }
    }

    // TODO: Check if the player has disconnected or hit the Dequeue button

    private static void CreateMatch(RemotePlayerConnection p1, RemotePlayerConnection p2)
    {
        var match = new Match(p1, p2);
        activeMatches.Add(match);

        ServerMessageHelper.Log($"Match created for {p1.PlayerName}|WHITE and {p2.PlayerName}|BLACK.");
    }

    /// <summary>
    /// Looks for an active match with a provided RemotePlayerConnection.
    /// Ends the match if it exists and sends the MATCH_END message to both players
    /// with info on who won and why
    /// </summary>
    public static void EndMatchWithPlayer(RemotePlayerConnection player)
    {
        ServerMessageHelper.Log("EndMatchWithPlayer() hit");
        for (int i = 0; i < activeMatches.Count; i++)
        {
            Match match = activeMatches[i];
            if (match.IsPlayerInThisMatch(player))
            {
                ServerMessageHelper.Log("Active match found.");
                match.End();
                activeMatches.RemoveAt(i);
                ServerMessageHelper.Log("Match Removed from active matches");
            }
        }
    }
}
