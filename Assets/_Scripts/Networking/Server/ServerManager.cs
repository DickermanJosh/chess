using UnityEngine;

public class ServerManager : MonoBehaviour
{
    [SerializeField] private int port = 24355;

    private TCPServer server;

    private void Awake()
    {
        Debug.Log("[ServerManager] Starting server on port " + port);
        server = new TCPServer(port);
        server.StartServer();
    }

    private void OnDestroy()
    {
        Debug.Log("[ServerManager] Stopping server...");
        server?.StopServer();
    }
}
