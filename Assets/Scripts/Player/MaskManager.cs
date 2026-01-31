using UnityEngine;
using System.Collections;

public class MaskManager : MonoBehaviour
{
    [Header("Mask Settings")]
    public float maxMaskTime = 10f;
    public float regenSpeed = 2f;
    public float regenDelay = 2f;

    [Header("Animation")]
    public Animator maskAnimator;
    public GameObject maskVisual; // <-- Drag mask model here
    public float animationDuration = 0.5f; // match animation length

    public bool isMaskOn { get; private set; }

    private float currentMaskTime;
    private float regenDelayTimer = 0f;
    private bool maskEmpty = false;
    private bool isAnimating = false;

    private void Start()
    {
        currentMaskTime = maxMaskTime;

        // Hide mask at start
        if (maskVisual != null)
            maskVisual.SetActive(false);
    }

    private void Update()
    {
        UpdateMaskTime();
    }

    public void ToggleMask()
    {
        if (isAnimating) return;
        if (!isMaskOn && currentMaskTime == 0f) return;

        StartCoroutine(PlayMaskToggle());
    }

    IEnumerator PlayMaskToggle()
    {
        isAnimating = true;

        if (!isMaskOn)
        {
            // SHOW mask before ON animation
            if (maskVisual != null)
                maskVisual.SetActive(true);

            maskAnimator.SetTrigger("OnMask");
            isMaskOn = true;
        }
        else
        {
            // Play OFF animation first
            maskAnimator.SetTrigger("OffMask");
            isMaskOn = false;
        }

        Debug.Log(isMaskOn ? "Mask ON" : "Mask OFF");

        // Wait until animation finishes
        yield return new WaitForSeconds(animationDuration);

        // Hide mask AFTER OFF animation finishes
        if (!isMaskOn && maskVisual != null)
            maskVisual.SetActive(false);

        isAnimating = false;
    }

    void UpdateMaskTime()
    {
        if (isMaskOn)
        {
            currentMaskTime -= Time.deltaTime;

            if (currentMaskTime <= 0f)
            {
                currentMaskTime = 0f;
                isMaskOn = false;
                regenDelayTimer = regenDelay;
                maskEmpty = true;
                Debug.Log("Mask ran out, waiting to regen");

                // Hide mask when forced off
                if (maskVisual != null)
                    maskVisual.SetActive(false);
            }
        }
        else if (maskEmpty)
        {
            if (regenDelayTimer > 0f)
            {
                regenDelayTimer -= Time.deltaTime;
            }
            else
            {
                currentMaskTime += regenSpeed * Time.deltaTime;
                currentMaskTime = Mathf.Clamp(currentMaskTime, 0f, maxMaskTime);

                if (currentMaskTime >= maxMaskTime)
                    maskEmpty = false;
            }
        }
    }
}
