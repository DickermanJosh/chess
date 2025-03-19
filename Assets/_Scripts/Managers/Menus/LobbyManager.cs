using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private Button enterQueueButton;
    [SerializeField] private Button returnToMenu;

    private void Awake()
    {
        enterQueueButton.onClick.AddListener(OnClickQueueUp);
        returnToMenu.onClick.AddListener(OnClickReturnToMenu);
    }

    private void OnClickQueueUp()
    {
        ClientMessageHelper.SendQueueUp();
    }

    private void OnClickReturnToMenu()
    {
        PlayerIdentity.Client.Close();
        SceneLoader.Instance.LoadScene(SceneLoader.MainMenu);
    }
}
