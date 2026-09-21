using Opera;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button onlineGameButton, aiGameButton, localGameButton, quitButton;
    [SerializeField] private string ipAddress = "127.0.0.1";
    [SerializeField] private int port = 24355;
    [SerializeField] private TextMeshProUGUI helloText;
    [SerializeField] private InputField nameInput;
    [SerializeField] private Button submitButton;
    private TMP_InputField playerName;
    private TCPClient client;

    private void Awake()
    {
        PlayerIdentity.InitializeIdentity();
        foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) canvas.gameObject.SetActive(false);
        foreach (var backdrop in FindObjectsByType<SpaceBackgroundRenderer>(FindObjectsSortMode.None)) backdrop.gameObject.SetActive(false);
        var root = OperaTheme.Canvas(transform, "Chess / Main menu").transform;
        var background = OperaTheme.Box(root, "Walnut background", OperaTheme.Background, true, true); OperaTheme.Stretch(background);
        background.GetComponent<Image>().sprite = OperaTheme.Wood;
        var hero = OperaTheme.Box(root, "Title background", OperaTheme.Background, true);
        hero.anchorMin = Vector2.zero; hero.anchorMax = new Vector2(.55f, 1); hero.offsetMin = hero.offsetMax = Vector2.zero;
        var title = new GameObject("Title", typeof(RectTransform)).GetComponent<RectTransform>();
        title.SetParent(hero, false); title.anchorMin = new Vector2(0, .5f); title.anchorMax = new Vector2(1, .5f);
        title.sizeDelta = new Vector2(-120, 190);
        OperaTheme.Label(title, "Chess", 0, 150, 108, 0, true);
        var line = OperaTheme.Box(title, "Brass rule", OperaTheme.Brass); OperaTheme.Place(line, 160, 1, 0);

        var card = OperaTheme.Box(root, "Main menu", OperaTheme.Surface, true);
        card.anchorMin = card.anchorMax = new Vector2(.765f, .5f); card.sizeDelta = new Vector2(438, 540);
        var avatar = OperaTheme.Avatar(card, false); avatar.anchorMin = avatar.anchorMax = new Vector2(0, 1);
        avatar.anchoredPosition = new Vector2(53, -52);
        helloText = OperaTheme.Label(card, PlayerIdentity.PlayerName, 35, 36, 27, 60, true);
        helloText.enableAutoSizing = true; helloText.fontSizeMin = 17; helloText.fontSizeMax = 27; helloText.textWrappingMode = TextWrappingModes.NoWrap; helloText.overflowMode = TextOverflowModes.Ellipsis;
        helloText.rectTransform.offsetMin = new Vector2(90, helloText.rectTransform.offsetMin.y);
        aiGameButton = OperaTheme.Button(card, "Play computer", () => SceneLoader.Instance.LoadScene(SceneLoader.AIGame), true);
        OperaTheme.Place(aiGameButton.GetComponent<RectTransform>(), 110, 57, 60);
        aiGameButton.GetComponentInChildren<TMP_Text>().font = OperaTheme.Serif;
        aiGameButton.GetComponentInChildren<TMP_Text>().fontSize = 27;
        onlineGameButton = OperaTheme.Button(card, "Play online", OnConnectClicked); OperaTheme.Place(onlineGameButton.GetComponent<RectTransform>(), 184, 40, 60);
        localGameButton = OperaTheme.Button(card, "Local game  /  coming soon", null); OperaTheme.Place(localGameButton.GetComponent<RectTransform>(), 236, 40, 60);
        localGameButton.interactable = false; // This scene has no local game implementation yet.
        var divider = OperaTheme.Box(card, "Divider", new Color(.7f, .6f, .4f, .25f)); OperaTheme.Place(divider, 302, 1, 60);
        OperaTheme.Label(card, "PLAYER NAME", 325, 24, 11, 60).color = OperaTheme.Brass;
        var field = OperaTheme.Box(card, "Player name", OperaTheme.Recess, false, true); OperaTheme.Place(field, 357, 41, 60);
        playerName = field.gameObject.AddComponent<TMP_InputField>(); playerName.characterLimit = 24;
        playerName.textViewport = field;
        var inputText = OperaTheme.Label(field, "", 0, 41, 16, 20); OperaTheme.Stretch(inputText.rectTransform, 10);
        playerName.textComponent = inputText; playerName.text = PlayerIdentity.PlayerName;
        playerName.onEndEdit.AddListener(SaveName);
        submitButton = OperaTheme.Button(card, "Save name", () => SaveName(playerName.text)); OperaTheme.Place(submitButton.GetComponent<RectTransform>(), 410, 35, 60);
        quitButton = OperaTheme.Button(card, "Quit", () => Application.Quit()); OperaTheme.Place(quitButton.GetComponent<RectTransform>(), 474, 36, 60);
    }
    private void SaveName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        PlayerIdentity.SetPlayerName(value.Trim());
        helloText.text = PlayerIdentity.PlayerName;
    }
    private void OnConnectClicked()
    {
        client = new TCPClient(); client.Connect(ipAddress, port); PlayerIdentity.SetTcpClient(client);
        SceneLoader.Instance.LoadScene(SceneLoader.Lobby);
    }
}
