using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    private static float elapsedTime = 0f;

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        timerText.text =
            minutes.ToString("00") + ":" +
            seconds.ToString("00");
    }

    public static float GetTime()
    {
        return elapsedTime;
    }

    public static void SetTime(float time)
    {
        elapsedTime = time;
    }

    public static void ResetTimer()
    {
        elapsedTime = 0f;
    }
}