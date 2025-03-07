using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button onlineGameButton;
    [SerializeField] private Button aiGameButton;
    [SerializeField] private Button localGameButton;
    [SerializeField] private Button quitButton;

    [Header("Network Settings")]
    [SerializeField] private string ipAddress = "127.0.0.1";
    [SerializeField] private int port = 24355;

    [Header("Player Prefs")]
    [SerializeField] private TextMeshProUGUI helloText;
    [SerializeField] private InputField nameInput;
    [SerializeField] private Button submitButton;

    private TCPClient client;

    private void Awake()
    {
        onlineGameButton.onClick.AddListener(OnConnectClicked);
        aiGameButton.onClick.AddListener(OnAIGameClicked);
        localGameButton.onClick.AddListener(OnLocalGameClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        submitButton.onClick.AddListener(OnSubmitClicked);

        helloText.text = $"Hello, {PlayerIdentity.PlayerName}";
    }

    private void OnConnectClicked()
    {
        Debug.Log("[MainMenu] Connecting to " + ipAddress + ":" + port);

        client = new TCPClient();
        client.Connect(ipAddress, port);
        PlayerIdentity.SetTcpClient(client);

        SceneLoader.Instance.LoadScene(SceneLoader.Lobby);
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

    private void OnSubmitClicked()
    {
        if (nameInput.text.Length < 1)
        {
            return;
        }

        PlayerIdentity.SetPlayerName(nameInput.text);
        helloText.text = $"Hello, {PlayerIdentity.PlayerName}";
    }
}
