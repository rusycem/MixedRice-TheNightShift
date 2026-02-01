using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private CanvasGroup[] comicPanels;

    [Header("Global Fade Overlay")]
    [SerializeField] private CanvasGroup finalFadeOverlay;

    [Header("Movement Settings")]
    [SerializeField] private float slideDistance = 120f; 
    [SerializeField] private float zoomInScale = 1.15f;
    [SerializeField] private float zoomOutStartScale = 1.25f;
    [Range(1f, 1.5f)]
    [SerializeField] private float baseScale = 1.1f; // Prevents borders by making image slightly larger

    [Header("Audio Settings")]
    [SerializeField] private AudioSource bgmSource;

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 2.0f; 
    [SerializeField] private float displayDuration = 3.5f; 
    [SerializeField] private float finalWaitTime = 2.0f;
    [SerializeField] private string nextSceneName = "Level1";

    [Header("Skip Option")]
    [SerializeField] private KeyCode skipKey = KeyCode.Space;

    private CanvasGroup currentActivePanel;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        foreach (CanvasGroup panel in comicPanels) panel.alpha = 0;
        if (finalFadeOverlay != null) finalFadeOverlay.alpha = 0;
        if (bgmSource != null && !bgmSource.isPlaying) bgmSource.Play();

        StartCoroutine(PlayComicSequence());
    }

    IEnumerator PlayComicSequence()
    {
        for (int i = 0; i < comicPanels.Length; i++)
        {
            CanvasGroup nextPanel = comicPanels[i];
            float totalPanelTime = fadeDuration + displayDuration;

            // Start fading out the previous panel if it exists
            if (currentActivePanel != null) StartCoroutine(SimpleFade(currentActivePanel, 0, fadeDuration));
            
            currentActivePanel = nextPanel;
            yield return StartCoroutine(AnimatePanelSmooth(nextPanel, i, totalPanelTime));
        }

        yield return new WaitForSeconds(finalWaitTime);

        if (finalFadeOverlay != null)
        {
            if (bgmSource != null) StartCoroutine(FadeAudioOut());
            yield return StartCoroutine(SimpleFade(finalFadeOverlay, 1, fadeDuration));
        }

        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator AnimatePanelSmooth(CanvasGroup cg, int index, float totalDuration)
    {
        float elapsed = 0;
        RectTransform rt = cg.GetComponent<RectTransform>();
        
        Vector2 originalPos = rt.anchoredPosition;
        Vector2 startPos = originalPos;
        Vector2 endPos = originalPos;
        
        // Apply baseScale to start and end so there are NO borders
        Vector3 startScale = Vector3.one * baseScale;
        Vector3 endScale = Vector3.one * baseScale;

        switch (index)
        {
            case 0: // Pan Left
                startPos = originalPos + new Vector2(slideDistance, 0);
                break;
            case 1: // Pan Right
                startPos = originalPos + new Vector2(-slideDistance, 0);
                break;
            case 2: // Zoom In
                endScale = new Vector3(zoomInScale, zoomInScale, 1);
                break;
            case 3: // Zoom Out
                startScale = new Vector3(zoomOutStartScale, zoomOutStartScale, 1);
                endScale = Vector3.one * baseScale;
                break;
        }

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;
            float globalT = elapsed / totalDuration;
            float fadeT = Mathf.Clamp01(elapsed / fadeDuration);
            
            cg.alpha = Mathf.SmoothStep(0, 1, fadeT);

            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, globalT);
            cg.transform.localScale = Vector3.Lerp(startScale, endScale, globalT);

            yield return null;
        }
    }

    // Reuse for both fade out of old panels and fade in of black overlay
    IEnumerator SimpleFade(CanvasGroup cg, float targetAlpha, float duration)
    {
        float elapsed = 0;
        float startAlpha = cg.alpha;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }

    IEnumerator FadeAudioOut()
    {
        float startVol = bgmSource.volume;
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, 0, elapsed / fadeDuration);
            yield return null;
        }
        bgmSource.Stop();
    }

    void Update()
    {
        if (Input.GetKeyDown(skipKey)) SceneManager.LoadScene(nextSceneName);
    }
}