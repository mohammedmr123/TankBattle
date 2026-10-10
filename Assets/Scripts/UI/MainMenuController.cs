using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Tooltip("Build index of the first level.")]
    public int firstLevelBuildIndex = 1;

    public void OnPlayButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevelBuildIndex);
    }

    public void OnOptionsButton()
    {
        OptionsMenuUI.Open();
    }

    public void OnQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

