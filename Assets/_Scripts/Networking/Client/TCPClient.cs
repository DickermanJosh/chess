using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/// <summary>
/// Minimal TCP client. Connects to a server, can send messages, 
/// and receives data from the server.
/// </summary>
public class TCPClient
{
    private TcpClient client;
    private NetworkStream stream;
    private byte[] buffer = new byte[1024];
    private bool isConnected = false;

    public bool IsConnected => isConnected;

    // Fired when a new message arrives
    public event Action<string> OnMessageReceived;

    public void Connect(string serverIP, int port)
    {
        try
        {
            client = new TcpClient();
            client.BeginConnect(serverIP, port, OnConnected, null);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Client] Connect error: " + ex.Message);
        }
    }

    private void OnConnected(IAsyncResult ar)
    {
        try
        {
            client.EndConnect(ar);
            stream = client.GetStream();
            isConnected = true;
            Debug.Log("[Client] Connected to server.");

            // Begin reading from server
            stream.BeginRead(buffer, 0, buffer.Length, OnDataReceived, null);

            // Sending ID's from the client like this is not secure, but for a small-scale
            // application like this one that doesn't yet have a server-side db - it works.
            // ------------------ Send ID + name handshake -----------------------
            string message = ClientMessageHelper.GetConnectId();
            SendMessage(message);
            // -------------------------------------------------------------------
        }
        catch (Exception ex)
        {
            Debug.LogError("[Client] OnConnected error: " + ex.Message);
        }
    }

    public void SendMessage(string message)
    {
        if (!isConnected) return;
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Client] SendMessage error: " + ex.Message);
        }
    }

    private void OnDataReceived(IAsyncResult ar)
    {
        if (!isConnected) return;
        try
        {
            int bytesRead = stream.EndRead(ar);
            if (bytesRead <= 0)
            {
                // Disconnected
                Debug.Log("[Client] Server disconnected.");
                Close();
                return;
            }

            string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Debug.Log($"[Client] Received: {msg}");
            OnMessageReceived?.Invoke(msg);
            ClientMessageHelper.HandleReceivedMessage(msg);

            // Keep reading
            stream.BeginRead(buffer, 0, buffer.Length, OnDataReceived, null);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Client] OnDataReceived error: " + ex.Message);
            Close();
        }
    }



    public void Close()
    {
        isConnected = false;
        stream?.Close();
        client?.Close();
    }
}
