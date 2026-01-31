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

    public bool isMaskOn { get; private set; }

    private float currentMaskTime;
    private float regenDelayTimer = 0f;
    private int lastSecond = -1;
    private bool isRegenerating = false;
    private bool maskEmpty = false;
    private bool maskEmpty = false;
    private bool isAnimating = false;

    private int lastSecond = -1;

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
        DebugCountdown();
    }


    void UpdateUI()
    {
        if (maskFillImage != null)
        {
            maskFillImage.fillAmount = currentMaskTime / maxMaskTime;
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
                isMaskOn = false;
                regenDelayTimer = regenDelay;
                maskEmpty = true;
                isRegenerating = false;
                Debug.Log("Mask ran out, waiting to regen");

                if (maskVisual != null)
                    maskVisual.SetActive(false);
            }
        }
        else if (maskEmpty)
        {
            if (maskEmpty)
            {
                if (regenDelayTimer > 0f)
                {
                    regenDelayTimer -= Time.deltaTime;
                }
                else
                {
                    isRegenerating = true;
                    currentMaskTime += regenSpeed * Time.deltaTime;
                    currentMaskTime = Mathf.Clamp(currentMaskTime, 0f, maxMaskTime);

                    if (currentMaskTime >= maxMaskTime)
                    {
                        currentMaskTime = maxMaskTime;
                        isRegenerating = false;
                        maskEmpty = false;
                    }
                }
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

    public void ToggleMask()
    {
        if (!isMaskOn && currentMaskTime == 0f)
            return;

        isMaskOn = !isMaskOn;
        Debug.Log(isMaskOn ? "Mask ON" : "Mask OFF");
    }

    void DebugCountdown()
    {
        int second = Mathf.CeilToInt(currentMaskTime);
        if (second == lastSecond) return;

        lastSecond = second;

        string state =
            isMaskOn ? "Mask Draining" :
            (maskEmpty && regenDelayTimer > 0f) ? "Waiting to Regen" :
            (isRegenerating) ? "Mask Regenerating" :
            (maskEmpty) ? "Mask Regenerating" :
            "Mask Full";

        Debug.Log($"{state}: {second}s");
    }
}