using UnityEngine;
using UnityEngine.UI;

public class RenderResignButton : MonoBehaviour
{
    [SerializeField] private Button resignButton;

    private void Awake()
    {
        resignButton.onClick.AddListener(OnResignClicked);
    }

    private void OnResignClicked()
    {
        ClientMessageHelper.SendResign();
    }
}
