using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    public float scaleAmount = 1.15f;
    public float slideAmount = 10f;
    public float animationDuration = 0.2f;

    private Vector3 originalScale;
    private Vector2 originalPosition;
    
    private RectTransform rectTransform;
    private Coroutine activeRoutine;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        originalScale = transform.localScale;
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopCurrentAnimation();
        Vector3 targetScale = originalScale * scaleAmount;
        Vector2 targetPos = originalPosition + new Vector2(-slideAmount, 0f);
        
        activeRoutine = StartCoroutine(AnimateButton(targetScale, targetPos));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopCurrentAnimation();
        
        activeRoutine = StartCoroutine(AnimateButton(originalScale, originalPosition));
    }

    private void StopCurrentAnimation()
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);
    }

    IEnumerator AnimateButton(Vector3 targetScale, Vector2 targetPos)
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector2 startPos = rectTransform.anchoredPosition;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            float curve = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = Vector3.Lerp(startScale, targetScale, curve);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, curve);
            
            yield return null;
        }

        transform.localScale = targetScale;
        rectTransform.anchoredPosition = targetPos;
    }
}
