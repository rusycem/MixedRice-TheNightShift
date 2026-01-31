using UnityEngine;
using UnityEngine.UI; // 1. ADDED: Required for UI

public class MaskManager : MonoBehaviour
{
    [Header("UI Settings")]
    public Image maskFillImage; // 2. ADDED: Drag your Radial Image here

    [Header("Mask Settings")]
    public float maxMaskTime = 10f;
    public float regenSpeed = 2f;
    public float regenDelay = 2f;

    public bool isMaskOn { get; private set; }

    private float currentMaskTime;
    private float regenDelayTimer = 0f;
    private int lastSecond = -1;
    private bool isRegenerating = false;
    private bool maskEmpty = false;

    private void Start()
    {
        currentMaskTime = maxMaskTime;
    }

    private void Update()
    {
        UpdateMaskTime();
        UpdateUI(); // 3. ADDED: Update the visual bar every frame
        DebugCountdown();
    }

    // 4. ADDED: The UI Logic
    void UpdateUI()
    {
        if (maskFillImage != null)
        {
            // Calculates the percentage (0.0 to 1.0)
            maskFillImage.fillAmount = currentMaskTime / maxMaskTime;
        }
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
            }
        }
        else
        {
            // Only start regen if mask had emptied
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
            "Mask Full";

        Debug.Log($"{state}: {second}s");
    }
}