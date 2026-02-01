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

    [Header("Mask Audio")]
    public AudioClip maskClip;

    public bool isMaskOn { get; private set; }

    private float currentMaskTime;
    private float regenDelayTimer = 0f;
    private int lastSecond = -1;
    private bool isRegenerating = false;
    private bool maskEmpty = false;
    private bool isAnimating = false;

    private void Start()
    {
        currentMaskTime = maxMaskTime;

        if (maskVisual != null)
            maskVisual.SetActive(false);
    }

    private void Update()
    {
        UpdateMaskTime();
        UpdateUI();
        //DebugCountdown();
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
        if (!isMaskOn && currentMaskTime == 0f) return;

        StartCoroutine(PlayMaskToggle());
    }

    IEnumerator PlayMaskToggle()
    {
        isAnimating = true;

        if (!isMaskOn)
        {
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

        // Hide mask AFTER OFF animation
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

                // Turn off mask & trigger OFF animation
                isMaskOn = false;
                regenDelayTimer = regenDelay;
                maskEmpty = true;
                isRegenerating = false;

                Debug.Log("Mask ran out, playing OFF animation");

                if (!isAnimating)
                    StartCoroutine(AutoMaskOff());
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
                {
                    currentMaskTime = maxMaskTime;
                    maskEmpty = false;
                    isRegenerating = false;
                }
            }
        }
    }

    IEnumerator AutoMaskOff()
    {
        isAnimating = true;

        maskAnimator.SetTrigger("OffMask");

        yield return new WaitForSeconds(animationDuration);

        if (maskVisual != null)
            maskVisual.SetActive(false);

        isAnimating = false;
    }

}
