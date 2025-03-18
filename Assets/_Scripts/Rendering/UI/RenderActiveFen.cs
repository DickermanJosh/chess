using TMPro;
using UnityEngine;

public class RenderActiveFen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fenText;
    void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StateUpdated += UpdateDisplayedFen;
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StateUpdated -= UpdateDisplayedFen;
        }
    }
    private void UpdateDisplayedFen()
    {
        fenText.text = $"FEN: {GameManager.Instance.GameState.CurrentFen}";
    }
}
