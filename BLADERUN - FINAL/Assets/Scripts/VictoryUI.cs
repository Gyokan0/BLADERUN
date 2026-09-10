using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
{
    [SerializeField] private GameObject endScreen;

    public void ShowEndScreen()
    {
        if (endScreen != null)
            endScreen.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("0MainMenu");
    }
}