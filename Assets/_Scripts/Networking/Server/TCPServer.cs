using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/// <summary>
/// Minimal TCP server. Listens on a specified port, accepts clients,
/// receives messages, and broadcasts them to all connected clients.
/// </summary>
public class TCPServer
{
    private TcpListener listener;
    private List<TcpClient> clients = new List<TcpClient>();
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

            Debug.Log($"[Server] Started on port {port}.");
            // Begin accepting clients asynchronously
            listener.BeginAcceptTcpClient(OnClientConnected, null);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Server] Could not start: " + ex.Message);
        }
    }

    public void StopServer()
    {
        isRunning = false;
        try
        {
            listener?.Stop();
            foreach (var c in clients)
            {
                c.Close();
            }
            clients.Clear();
        }
        catch (Exception ex)
        {
            Debug.LogError("[Server] Error stopping server: " + ex.Message);
        }
    }

    private void OnClientConnected(IAsyncResult ar)
    {
        if (!isRunning) return;

        try
        {
            TcpClient client = listener.EndAcceptTcpClient(ar);
            clients.Add(client);
            Debug.Log("[Server] New client connected.");
            Debug.Log($"[Server] <Serving ({clients.Count}) active connections>");

            // Begin reading from this client
            StartReadingClient(client);

            // Keep accepting new clients
            listener.BeginAcceptTcpClient(OnClientConnected, null);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Server] Error accepting client: " + ex.Message);
        }
    }

    private void StartReadingClient(TcpClient client)
    {
        var state = new ClientState(client);
        client.GetStream().BeginRead(state.buffer, 0, state.buffer.Length, OnDataReceived, state);
    }

    private void OnDataReceived(IAsyncResult ar)
    {
        if (!isRunning) return;

        ClientState state = (ClientState)ar.AsyncState;
        try
        {
            int bytesRead = state.client.GetStream().EndRead(ar);
            if (bytesRead <= 0)
            {
                // Client disconnected
                RemoveClient(state.client);
                return;
            }

            string message = Encoding.UTF8.GetString(state.buffer, 0, bytesRead);
            Debug.Log($"[Server] Received: {message}");

            // BROADCAST or handle logic here
            BroadcastMessage(message);

            // Continue reading from this client
            state.client.GetStream().BeginRead(state.buffer, 0, state.buffer.Length, OnDataReceived, state);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Server] Error reading data: " + ex.Message);
            RemoveClient(state.client);
        }
    }

    private void BroadcastMessage(string msg)
    {
        byte[] data = Encoding.UTF8.GetBytes(msg);
        foreach (var c in clients)
        {
            try
            {
                c.GetStream().Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Debug.LogError("[Server] Error broadcasting: " + ex.Message);
            }
        }
    }

    private void RemoveClient(TcpClient client)
    {
        Debug.Log("[Server] Removing client...");
        clients.Remove(client);
        client.Close();
        Debug.Log("[Server] Removed client.");
        Debug.Log($"[Server] <Serving ({clients.Count}) active connections>");
    }

    // Helper class to track reading state
    private class ClientState
    {
        public TcpClient client;
        public byte[] buffer;

        public ClientState(TcpClient c)
        {
            client = c;
            buffer = new byte[1024];
        }
    }
}
