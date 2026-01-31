using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Main Menu Scene")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Fade Transition")]
    public CanvasGroup fadeCanvasGroup;
    public float transitionDuration = 1f;

    bool isLoading = false;

    public void BackToMainMenu()
    {
        if (!isLoading)
        {
            StartCoroutine(LoadMainMenuSequence());
        }
    }

    IEnumerator LoadMainMenuSequence()
    {
        isLoading = true;

        // Resume time if paused
        Time.timeScale = 1f;

        yield return StartCoroutine(Fade(1f));

        SceneManager.LoadScene(mainMenuSceneName);
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float elapsed = 0;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime; // works even if paused
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / transitionDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}
