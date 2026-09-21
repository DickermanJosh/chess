using Opera;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private Button enterQueueButton, returnToMenu;
    private TextMeshProUGUI status;
    private void Awake()
    {
        foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) canvas.gameObject.SetActive(false);
        var root = OperaTheme.Canvas(transform, "Opera / Lobby").transform;
        var background = OperaTheme.Box(root, "Lobby", OperaTheme.Background, true, true); OperaTheme.Stretch(background);
        var card = OperaTheme.Box(root, "Matchmaking", OperaTheme.Surface, true);
        card.anchorMin = card.anchorMax = new Vector2(.5f, .5f); card.sizeDelta = new Vector2(500, 420);
        var avatar = OperaTheme.Avatar(card, false, 56); avatar.anchorMin = avatar.anchorMax = new Vector2(.5f, 1); avatar.anchoredPosition = new Vector2(0, -62);
        var title = OperaTheme.Label(card, "Lobby", 114, 49, 38, 60, true); title.alignment = TextAlignmentOptions.Center;
        status = OperaTheme.Label(card, "", 181, 55, 16, 60); status.color = OperaTheme.Muted; status.alignment = TextAlignmentOptions.Center;
        enterQueueButton = OperaTheme.Button(card, "Find a game", OnClickQueueUp, true);
        OperaTheme.Place(enterQueueButton.GetComponent<RectTransform>(), 258, 46, 80);
        returnToMenu = OperaTheme.Button(card, "Main menu", OnClickReturnToMenu);
        OperaTheme.Place(returnToMenu.GetComponent<RectTransform>(), 323, 38, 80);
    }
    private void OnClickQueueUp()
    {
        ClientMessageHelper.SendQueueUp(); status.text = "Waiting for an opponent..."; enterQueueButton.interactable = false;
    }
    private void OnClickReturnToMenu()
    {
        PlayerIdentity.Client?.Close(); SceneLoader.Instance.LoadScene(SceneLoader.MainMenu);
    }
}
