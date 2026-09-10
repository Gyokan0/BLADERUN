using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;

        PlayerHealth health = FindAnyObjectByType<PlayerHealth>();

        if (health != null)
            health.RespawnAtCheckpoint();

        SlimeEnemy[] slimes = FindObjectsByType<SlimeEnemy>(
            FindObjectsInactive.Include
        );

        foreach (SlimeEnemy slime in slimes)
            slime.ResetEnemy();

        BatEnemy[] bats = FindObjectsByType<BatEnemy>(
            FindObjectsInactive.Include
        );

        foreach (BatEnemy bat in bats)
            bat.ResetEnemy();

        WolfEnemy[] wolves = FindObjectsByType<WolfEnemy>(
            FindObjectsInactive.Include
        );

        foreach (WolfEnemy wolf in wolves)
            wolf.ResetEnemy();

        KingBoss[] kings = FindObjectsByType<KingBoss>(
            FindObjectsInactive.Include
        );

        foreach (KingBoss king in kings)
            king.ResetEnemy();

        gameOverPanel.SetActive(false);
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;

        try
        {
            SaveSystem.SaveScene(SceneManager.GetActiveScene().name);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Game Over save error: " + e.Message);
        }
        SceneManager.LoadScene("0MainMenu");
    }
}