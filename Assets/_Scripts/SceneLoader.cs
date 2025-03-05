using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // public static Animator transition;
    public static float transitionTime = 0.01f;
    // public static float transitionTime = 1f;

    public static readonly string ServerScene = "ServerScene";
    public static readonly string MainMenu = "MainMenu";
    public static readonly string Lobby = "Lobby";
    public static readonly string OnlineGame = "OnlineGame";
    public static readonly string AIGame = "AIGame";
    public static readonly string LocalGame = "LocalGame";

    private static SceneLoader _instance;
    public static SceneLoader Instance => _instance;
    private void Awake()
    {
        if (_instance is not null)
        {
            Destroy(_instance);
        }
        
        _instance = this;
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(DoTransition(sceneName));
    }

    private static IEnumerator DoTransition(string sceneName)
    {
        // transition.SetTrigger("Start"); TODO: add transition effect later

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(sceneName);
    }
}