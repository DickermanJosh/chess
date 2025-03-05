using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // public static Animator transition;
    public static float transitionTime = 0.01f;
    // public static float transitionTime = 1f;

    public static readonly string MainMenu = "MainMenu";
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

    public void LoadLevel(string levelName)
    {
        StartCoroutine(DoTransition(levelName));
    }

    private static IEnumerator DoTransition(string levelName)
    {
        // transition.SetTrigger("Start"); TODO: add transition effect later

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(levelName);
    }
}