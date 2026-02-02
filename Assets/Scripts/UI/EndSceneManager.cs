using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI endText;
    [SerializeField] private CanvasGroup fadeOverlay;

    [Header("Settings")]
    [SerializeField] private string[] textLines;
    [SerializeField] private string sceneName = "MainMenu";
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private float delayBetweenLines = 1.5f;
    [SerializeField] private float delayBeforeFade = 1.0f;
    [SerializeField] private float fadeDuration = 1.5f;

    [SerializeField] private bool clearTextBetweenLines = true;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private bool fadeAudio = true;

    void Start()
    {
        fadeOverlay.alpha = 0;
        endText.text = "";

        if (textLines.Length > 0)
        {
            StartCoroutine(RunEndSequence());
        }
    }

    IEnumerator RunEndSequence()
{
    for (int i = 0; i < textLines.Length; i++)
    {
        yield return StartCoroutine(TypewriterEffect(textLines[i]));

        yield return new WaitForSeconds(delayBetweenLines);

        bool isLastLine = (i == textLines.Length - 1);

        if (clearTextBetweenLines && !isLastLine)
        {
            endText.text = "";
        }
    }

    // 4. Short pause while the last line is still fully visible
    yield return new WaitForSeconds(delayBeforeFade);

    // 5. Start the smooth fade (this will now fade the text because it's still there!)
    yield return StartCoroutine(Fade(1));

    SceneManager.LoadScene(sceneName);
}

    IEnumerator TypewriterEffect(string text)
    {
        if (!clearTextBetweenLines && endText.text != "")
        {
            endText.text += "\n";
        }
        else
        {
            endText.text = "";
        }

        foreach (char c in text)
        {
            endText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeOverlay.alpha;
        float startVolume = (bgmSource != null) ? bgmSource.volume : 0;
        Color startTextColor = endText.color;
        float time = 0;

        fadeOverlay.blocksRaycasts = true; 

        while (time < fadeDuration)
        {
            time += Time.deltaTime; 
            float progress = time / fadeDuration;
            float easedProgress = Mathf.SmoothStep(0, 1, progress);

            // Fade Overlay
            fadeOverlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, easedProgress);
            
            // Fade Text
            float textAlpha = Mathf.Lerp(1, 1 - targetAlpha, easedProgress);
            endText.color = new Color(startTextColor.r, startTextColor.g, startTextColor.b, textAlpha);

            // --- ADDED: Fade Audio ---
            if (fadeAudio && bgmSource != null)
            {
                // If targetAlpha is 1 (fading to black), volume goes to 0
                bgmSource.volume = Mathf.Lerp(startVolume, 0, easedProgress);
            }

            yield return null;
        }

        fadeOverlay.alpha = targetAlpha;
    }
}
