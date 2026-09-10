using System.Collections;
using UnityEngine;

public class BiomeTitle : MonoBehaviour
{
    public CanvasGroup titleCanvasGroup;

    public float showDelay = 1f;
    public float fadeInDuration = 0.5f;
    public float stayDuration = 2f;
    public float fadeOutDuration = 0.5f;

    private void Start()
    {
        titleCanvasGroup.alpha = 0f;
        titleCanvasGroup.gameObject.SetActive(true);

        StartCoroutine(ShowTitle());
    }

    private IEnumerator ShowTitle()
    {
        yield return new WaitForSecondsRealtime(showDelay);

        yield return StartCoroutine(Fade(0f, 1f, fadeInDuration));

        yield return new WaitForSecondsRealtime(stayDuration);

        yield return StartCoroutine(Fade(1f, 0f, fadeOutDuration));

        titleCanvasGroup.gameObject.SetActive(false);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            titleCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                elapsedTime / duration
            );

            yield return null;
        }

        titleCanvasGroup.alpha = endAlpha;
    }
}