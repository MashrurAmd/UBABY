using UnityEngine;
using UnityEngine.SceneManagement; // Needed for scene reloading

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;

    public void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // Pause the game
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Resume time before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload current scene
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");

#if UNITY_EDITOR
        // If running inside Unity Editor, stop playing
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If running as a standalone build, quit
        Application.Quit();
#endif
    }
}

