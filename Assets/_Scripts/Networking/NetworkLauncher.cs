using UnityEngine;
using UnityEngine.UI;

public class NetworkLauncher : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private InputField ipInputField;   // user can type e.g. "127.0.0.1"

    private TCPServer server;
    private TCPClient client;

    private void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
    }

    private void OnHostClicked()
    {
        // Start the server
        server = new TCPServer(7777);
        server.StartServer();


        // Possibly load the game scene. 
        // Keep the server + client objects around (e.g. use DontDestroyOnLoad?)
        Debug.Log("[Launcher] Hosting game...");
    }

    private void OnJoinClicked()
    {
        string ip = ipInputField.text;
        client = new TCPClient();
        client.Connect(ip, 7777);

        Debug.Log("[Launcher] Joining game at " + ip);
        // load the game scene or do other logic
        SceneLoader.Instance.LoadLevel(SceneLoader.LocalGame);
    }
}
