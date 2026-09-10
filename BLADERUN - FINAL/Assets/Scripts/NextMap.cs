using UnityEngine;
using UnityEngine.SceneManagement;

public class NextMap : MonoBehaviour
{
    public string nextSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SaveSystem.SaveScene(nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }
    }
}