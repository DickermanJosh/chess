using UnityEngine;
using UnityEngine.SceneManagement;

public class BootManager : MonoBehaviour
{
    private void Awake()
    {
        // string[] args = System.Environment.GetCommandLineArgs();
        if (Application.isBatchMode)
        {
            // We are headless => load the server scene
            Debug.Log("[BootManager] Detected headless (batchmode). Loading Server scene...");
            SceneManager.LoadScene("ServerScene");
        }
        else
        {
            // We are not headless => load the client main menu
            Debug.Log("[BootManager] Detected non-headless. Loading Main Menu scene...");
       
            PlayerIdentity.InitializeIdentity();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
