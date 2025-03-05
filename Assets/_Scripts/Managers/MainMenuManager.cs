using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Buttons in Scene")]
    [SerializeField] private Button onlineGameButton;
    [SerializeField] private Button aiGameButton;
    [SerializeField] private Button localGameButton;
    [SerializeField] private Button quitButton;

    [Header("Network Settings")]
    [SerializeField] private string ipAddress = "127.0.0.1";
    [SerializeField] private int port = 24355;

    private TCPClient client;

    private void Awake()
    {
        onlineGameButton.onClick.AddListener(OnConnectClicked);
        aiGameButton.onClick.AddListener(OnAIGameClicked);
        localGameButton.onClick.AddListener(OnLocalGameClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnConnectClicked()
    {
        Debug.Log("[MainMenu] Connecting to " + ipAddress + ":" + port);
        client = new TCPClient();
        client.Connect(ipAddress, port);

        // TODO: Send this to a 'lobby' page that allows users to make / join rooms
        SceneLoader.Instance.LoadScene(SceneLoader.OnlineGame);
    }

    private void OnAIGameClicked()
    {

    }

    private void OnLocalGameClicked()
    {
        SceneLoader.Instance.LoadScene(SceneLoader.LocalGame);
    }


    private void OnQuitClicked()
    {
        Application.Quit();
    }
}
