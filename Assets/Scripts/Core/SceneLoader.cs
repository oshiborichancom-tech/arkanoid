using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public const string TitleSceneName = "TitleScene";
    public const string StageSelectSceneName = "StageSelectScene";
    public const string GameSceneName = "GameScene";

    public void LoadTitle()
    {
        LoadScene(TitleSceneName);
    }

    public void LoadStageSelect()
    {
        LoadScene(StageSelectSceneName);
    }

    public void LoadGame()
    {
        LoadScene(GameSceneName);
    }

    public void RestartCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("Scene name is empty.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
