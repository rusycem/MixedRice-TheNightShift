using UnityEngine;

public class JumpscareEffect : MonoBehaviour
{
    [Header("Settings")]
    public float shakeAmount = 10f; // How violently it shakes
    public float zoomSpeed = 1.5f;  // How fast it zooms into your face

    private Vector3 originalPos;
    private Vector3 originalScale;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPos = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;
    }

    void OnEnable()
    {
        // Reset whenever the panel turns on
        rectTransform.anchoredPosition = originalPos;
        rectTransform.localScale = originalScale;
    }

    void Update()
    {
        // 1. SHAKE EFFECT (Randomly jitter the position)
        float x = Random.Range(-shakeAmount, shakeAmount);
        float y = Random.Range(-shakeAmount, shakeAmount);
        rectTransform.anchoredPosition = originalPos + new Vector3(x, y, 0);

        // 2. ZOOM EFFECT (Slowly get bigger)
        //rectTransform.localScale += Vector3.one * zoomSpeed * Time.deltaTime;
    }
}