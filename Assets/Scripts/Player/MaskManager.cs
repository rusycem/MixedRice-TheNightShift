using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MaskManager : MonoBehaviour
{
    [Header("UI Settings")]
    public Image maskFillImage;

    [Header("Mask Settings")]
    public float maxMaskTime = 10f;
    public float regenSpeed = 2f;
    public float regenDelay = 2f;

    [Header("Animation")]
    public Animator maskAnimator;
    public GameObject maskVisual;
    public float animationDuration = 0.5f;

    // Public property for the AI to read
    public bool isMaskOn { get; private set; }

    private float currentMaskTime;
    private float regenDelayTimer = 0f;
    private bool maskEmpty = false;
    private bool isAnimating = false;

    private void Start()
    {
        currentMaskTime = maxMaskTime;
        if (maskVisual != null) maskVisual.SetActive(false);
    }

    private void Update()
    {
        UpdateMaskLogic(); // Renamed for clarity
        UpdateUI();
    }

    void UpdateUI()
    {
        if (maskFillImage != null)
        {
            maskFillImage.fillAmount = currentMaskTime / maxMaskTime;
        }
    }

    public void ToggleMask()
    {
        if (isAnimating) return;

        // Cannot turn on if empty
        if (!isMaskOn && (maskEmpty || currentMaskTime <= 0f)) return;

        StartCoroutine(PlayMaskToggle());
    }

    IEnumerator PlayMaskToggle()
    {
        isAnimating = true;

        if (!isMaskOn)
        {
            // TURN ON
            if (maskVisual != null) maskVisual.SetActive(true);
            if (maskAnimator) maskAnimator.SetTrigger("OnMask");
            isMaskOn = true;
            maskEmpty = false; // Reset empty flag immediately
        }
        else
        {
            // TURN OFF
            if (maskAnimator) maskAnimator.SetTrigger("OffMask");
            isMaskOn = false;
        }

        yield return new WaitForSeconds(animationDuration);

        // Cleanup after turning off
        if (!isMaskOn && maskVisual != null) maskVisual.SetActive(false);

        isAnimating = false;
    }

    void UpdateMaskLogic()
    {
        if (isMaskOn)
        {
            // --- DRAINING ---
            currentMaskTime -= Time.deltaTime;

            if (currentMaskTime <= 0f)
            {
                currentMaskTime = 0f;
                // Force toggle OFF if time runs out
                if (!isAnimating) StartCoroutine(PlayMaskToggle());

                maskEmpty = true;
                regenDelayTimer = regenDelay;
                Debug.Log("Mask ran out!");
            }
        }
        else
        {
            // --- REGENERATING ---
            // Only regen if we are not currently animating and logic allows it
            if (currentMaskTime < maxMaskTime)
            {
                if (regenDelayTimer > 0f)
                {
                    regenDelayTimer -= Time.deltaTime;
                }
                else
                {
                    currentMaskTime += regenSpeed * Time.deltaTime;
                    if (currentMaskTime > maxMaskTime)
                    {
                        currentMaskTime = maxMaskTime;
                        maskEmpty = false;
                    }
                }
            }
        }
    }
}