using UnityEngine;
using UnityEngine.UI;

public class NetworkLauncher : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    // [SerializeField] private InputField ipInputField;   // user can type e.g. "127.0.0.1" / localhost for now

    private TCPServer server;
    private TCPClient client;

    private void Start()
    {
        Screen.fullScreen = true;
        hostButton.onClick.AddListener(OnHostClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
        DontDestroyOnLoad(gameObject);
    }

    private void OnHostClicked()
    {
        // Start the server
        server = new TCPServer(24355);
        server.StartServer();


        // TODO: Load Server Scene 
        Debug.Log("[Launcher] Hosting game...");
        Screen.SetResolution(640, 360, false);
    }

    private void OnJoinClicked()
    {
        // string ip = ipInputField.text;
        string ip = "127.0.0.1";
        client = new TCPClient();
        client.Connect(ip, 24355);

        // TODO: Send the player to a loading screen then
        // 1. to the game if the connection is accepted
        // 2. back to menu with an error message if it is not
        // This needs to be done after client / host abstractions are made in the init scene
        Debug.Log("[Launcher] Joining game at " + ip);
        SceneLoader.Instance.LoadScene(SceneLoader.LocalGame);
    }
}
