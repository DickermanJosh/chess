using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/// <summary>
/// Minimal TCP server. Listens on a specified port (24355), accepts clients,
/// receives messages including CONNECT_ID, QUEUEUP, MOVE and sends appropriate responses to client
/// </summary>
public class TCPServer
{
    private TcpListener listener;
    private List<RemotePlayerConnection> connections = new List<RemotePlayerConnection>();
    private bool isRunning = false;

    private int port;

    public TCPServer(int port)
    {
        this.port = port;
    }

    public void StartServer()
    {
        try
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            isRunning = true;

            ServerMessageHelper.Log($"Started on port {port}.");
            // Begin accepting clients asynchronously
            listener.BeginAcceptTcpClient(OnClientConnected, null);
        }
        catch (Exception ex)
        {
            ServerMessageHelper.LogError("Could not start: " + ex.Message);
        }
    }

    public void StopServer()
    {
        isRunning = false;
        try
        {
            listener?.Stop();
            foreach (var c in connections)
            {
                c.TcpClient.Close();
            }
            connections.Clear();
        }
        catch (Exception ex)
        {
            ServerMessageHelper.LogError("Error stopping server: " + ex.Message);
        }
    }

    private void OnClientConnected(IAsyncResult ar)
    {
        if (!isRunning) return;

        try
        {
            TcpClient client = listener.EndAcceptTcpClient(ar);
            // clients.Add(client);

            // Create a new RemotePlayerConnection with a placeholder ID and name until
            // the CONNECT_ID message is received
            string dummyId = Guid.NewGuid().ToString();
            var conn = new RemotePlayerConnection(client, dummyId, "Unnamed");
            connections.Add(conn);

            ServerMessageHelper.Log("New client connected... Fetching ID and name.");
            ServerMessageHelper.Log($"<Serving ({connections.Count}) active connections>");

            // Begin reading from this client
            StartReadingClient(conn);

            // Keep accepting new clients
            listener.BeginAcceptTcpClient(OnClientConnected, null);
        }
        catch (Exception ex)
        {
            ServerMessageHelper.LogError($"Error accepting client: {ex.Message}");
        }
    }

    private void StartReadingClient(RemotePlayerConnection conn)
    {
        var state = new ClientState(conn);
        conn.Stream.BeginRead(state.buffer, 0, state.buffer.Length, OnDataReceived, state);
    }

    private void OnDataReceived(IAsyncResult ar)
    {
        if (!isRunning) return;

        ClientState state = (ClientState)ar.AsyncState;
        RemotePlayerConnection conn = state.connection;

        try
        {
            int bytesRead = conn.Stream.EndRead(ar);
            if (bytesRead <= 0)
            {
                // Client disconnected
                RemoveClient(conn);
                return;
            }

            string message = Encoding.UTF8.GetString(state.buffer, 0, bytesRead);
            ServerMessageHelper.Log($"Received from {conn.PlayerId}|{conn.PlayerName}: {message}");

            ServerMessageHelper.HandleReceivedMessage(conn, message);

            // Continue reading from this client
            conn.Stream.BeginRead(state.buffer, 0, state.buffer.Length, OnDataReceived, state);
        }
        catch (Exception ex)
        {
            ServerMessageHelper.LogError($"Error reading data from {conn.PlayerId}:{conn.PlayerName} " +
                $"\n Exception: {ex.Message}");
            RemoveClient(conn);
        }
    }


    private void BroadcastMessage(string msg)
    {
        byte[] data = Encoding.UTF8.GetBytes(msg);
        foreach (var c in connections)
        {
            try
            {
                c.Stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                ServerMessageHelper.LogError("Error broadcasting: " + ex.Message);
            }
        }
    }

    private void RemoveClient(RemotePlayerConnection conn)
    {
        ServerMessageHelper.Log($"Removing client {conn.PlayerName} with ID {conn.PlayerId}...");
        connections.Remove(conn);
        conn.TcpClient.Close();
        ServerMessageHelper.Log("Removed client.");
        ServerMessageHelper.Log($"<Serving ({connections.Count}) active connections>");
    }

    /// <summary>
    /// Helper class to track reading state
    /// Updated to store a RemotePlayerConnection instead of just a TcpClient
    /// </summary>
    private class ClientState
    {
        public RemotePlayerConnection connection;
        public byte[] buffer;

        public ClientState(RemotePlayerConnection c)
        {
            connection = c;
            buffer = new byte[1024];
        }
    }
}
