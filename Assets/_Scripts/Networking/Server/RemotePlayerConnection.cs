using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/// <summary>
/// RemotePlayerConnection is created by the server to represent an individual client connection
/// with access to a TcpClient and all the players info that is needed by the server.
/// </summary>
public class RemotePlayerConnection
{
    public TcpClient TcpClient { get; }
    public NetworkStream Stream { get; }
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public bool IsInMatch { get; set; }

    /// <summary>
    /// Called by the server when a connection is made, will initially take a dummy id and name
    /// which will be replaced once the client sends the CONNECT_ID message
    /// </summary>
    public RemotePlayerConnection(TcpClient client, string playerId, string playerName)
    {
        this.TcpClient = client;
        this.Stream = client.GetStream();
        this.PlayerId = playerId;
        this.PlayerName = playerName;
        this.IsInMatch = false;
    }

    /// <summary>
    /// Sends a message to the client represented by this RemotePlayerConnection
    /// </summary>
    public void Send(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        try
        {
            Stream.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            ServerMessageHelper.LogError($"Error sending message to {PlayerName}|{PlayerId}." +
                $"\nMessage Failed to send: {message}"
                + $"Logged Exception: {ex.Message}");
        }

    }
}
