using UnityEngine;

public class MaskManager : MonoBehaviour
{
    [Header("Mask Settings")]
    public float maxMaskTime = 10f;
    public float regenSpeed = 2f;
    public float regenDelay = 1.5f; // wait before regenerating

    public bool isMaskOn { get; private set; }

    float currentMaskTime;
    float regenDelayTimer = 0f;
    int lastSecond = -1;

    private void Start()
    {
        currentMaskTime = maxMaskTime;
    }

    private void Update()
    {
        UpdateMaskTime();
        DebugCountdown();
    }

    void UpdateMaskTime()
    {
        // Drain while mask is ON
        if (isMaskOn)
        {
            currentMaskTime -= Time.deltaTime;
        }
        else
        {
            // Wait before regen
            if (regenDelayTimer > 0f)
            {
                regenDelayTimer -= Time.deltaTime;
            }
            else
            {
                currentMaskTime += regenSpeed * Time.deltaTime;
            }
        }

        currentMaskTime = Mathf.Clamp(currentMaskTime, 0f, maxMaskTime);

        // Mask runs out
        if (isMaskOn && currentMaskTime == 0f)
        {
            isMaskOn = false;
            regenDelayTimer = regenDelay;
            Debug.Log("Mask 10s ran out — waiting to regen");
        }
    }

    public void ToggleMask()
    {
        // Can't turn on if empty
        if (!isMaskOn && currentMaskTime == 0f)
            return;

        isMaskOn = !isMaskOn;

        if (!isMaskOn)
            regenDelayTimer = regenDelay; // delay regen after manual off

        Debug.Log(isMaskOn ? "Mask ON" : "Mask OFF — waiting to regen");
    }

    // for debug console
    void DebugCountdown()
    {
        int second = Mathf.CeilToInt(currentMaskTime);
        if (second == lastSecond) return;

        lastSecond = second;

        string state =
            isMaskOn ? "Mask Draining" :
            regenDelayTimer > 0f ? "Waiting to Regen" :
            currentMaskTime < maxMaskTime ? "Mask Regenerating" :
            "Mask Full";

        Debug.Log($"{state}: {second}s");
    }
}
