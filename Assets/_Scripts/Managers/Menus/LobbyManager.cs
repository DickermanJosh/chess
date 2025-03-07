using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private Button enterQueueButton;

    private void Awake()
    {
        enterQueueButton.onClick.AddListener(OnClickQueueUp);
    }

    public void OnClickQueueUp()
    {
        string message = ClientMessageHelper.GetQueueUp();
        PlayerIdentity.Client.SendMessage(message);
    }
}
