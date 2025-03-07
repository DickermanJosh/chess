using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class RemotePlayerConnection
{
    public TcpClient TcpClient { get; }
    public NetworkStream Stream { get; }
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public bool IsInMatch { get; set; }

    public RemotePlayerConnection(TcpClient client, string playerId, string playerName)
    {
        this.TcpClient = client;
        this.Stream = client.GetStream();
        this.PlayerId = playerId;
        this.PlayerName = playerName;
        this.IsInMatch = false;
    }

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
