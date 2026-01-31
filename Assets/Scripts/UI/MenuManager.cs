using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public RectTransform mainMenuPanel;
    public RectTransform optionsPanel;
    public RectTransform creditsPanel;

    [Header("Scene Transition")]
    public CanvasGroup fadeCanvasGroup;
    public string nextSceneName;
    public float transitionDuration = 1f;

    [Header("Animation Settings")]
    public float slideDuration = 0.4f;

    private Vector2 showPos = Vector2.zero;
    private Vector2 leftHidePos = new Vector2(-2000f, 0f);
    private Vector2 rightHidePos = new Vector2(2000f, 0f);

    void Start()
    {
        if (mainMenuPanel) mainMenuPanel.anchoredPosition = showPos;
        if (optionsPanel) optionsPanel.anchoredPosition = rightHidePos;
        if (creditsPanel) creditsPanel.anchoredPosition = rightHidePos;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.gameObject.SetActive(true);
            StartCoroutine(Fade(0f));
        }
    }

    #region Public Button Methods

    public void StartGame()
    {
        StartCoroutine(LoadSceneSequence());
    }

    public void OpenOptions()
    {
        StartCoroutine(SlidePanel(mainMenuPanel, leftHidePos));
        StartCoroutine(SlidePanel(optionsPanel, showPos));
    }

    public void OpenCredits()
    {
        StartCoroutine(SlidePanel(mainMenuPanel, leftHidePos));
        StartCoroutine(SlidePanel(creditsPanel, showPos));
    }

    public void BackToMainMenu()
    {
        if (optionsPanel.anchoredPosition == showPos)
            StartCoroutine(SlidePanel(optionsPanel, rightHidePos));

        if (creditsPanel.anchoredPosition == showPos)
            StartCoroutine(SlidePanel(creditsPanel, rightHidePos));

        StartCoroutine(SlidePanel(mainMenuPanel, showPos));
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    #endregion

    #region Coroutines (Tweening Replacements)

    IEnumerator SlidePanel(RectTransform panel, Vector2 targetPos)
    {
        Vector2 startPos = panel.anchoredPosition;
        float elapsed = 0;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / slideDuration);
            panel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        panel.anchoredPosition = targetPos;
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float elapsed = 0;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / transitionDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }

    IEnumerator LoadSceneSequence()
    {
        yield return StartCoroutine(Fade(1f));
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    #endregion
}