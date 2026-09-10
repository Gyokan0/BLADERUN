using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject darkBackground;

    [SerializeField] private GameObject gameOverPanel;

    private bool isPaused = false;

    void Update()
    {
        if (gameOverPanel != null && gameOverPanel.activeSelf)
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                ContinueGame();
            else
                OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        darkBackground.SetActive(true);
        pausePanel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ContinueGame()
    {
        GameObject volumePanel = GameObject.Find("VolumePanel");

        if (volumePanel != null)
            volumePanel.SetActive(false);

        darkBackground.SetActive(false);
        pausePanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void ExitToMenu()
    {
        GameObject volumePanel = GameObject.Find("VolumePanel");

        if (volumePanel != null)
            volumePanel.SetActive(false);

        SaveSystem.SaveScene(
            SceneManager.GetActiveScene().name
        );

        Time.timeScale = 1f;
        SceneManager.LoadScene("0MainMenu");
    }
}