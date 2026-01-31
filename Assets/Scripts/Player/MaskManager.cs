using UnityEngine;

public class MaskManager : MonoBehaviour
{
    [Header("Mask Settings")]
    public float maxMaskTime = 10f;
    public float regenSpeed = 2f;
    public float regenDelay = 2f; // regenDelay

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
        if (isMaskOn)
        {
            currentMaskTime -= Time.deltaTime;

            // if mask runs out
            if (currentMaskTime <= 0f)
            {
                currentMaskTime = 0f;
                isMaskOn = false;
                regenDelayTimer = regenDelay; 
                Debug.Log("Mask ran out, waiting to regen");
            }
        }
        else
        {
            if (currentMaskTime == 0f && regenDelayTimer > 0f)
            {
                regenDelayTimer -= Time.deltaTime;
            }
            else if (currentMaskTime == 0f && regenDelayTimer <= 0f)
            {
                currentMaskTime += regenSpeed * Time.deltaTime;
                currentMaskTime = Mathf.Clamp(currentMaskTime, 0f, maxMaskTime);
            }
        }
    }

    public void ToggleMask()
    {
        // Can't turn on if empty
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
            (currentMaskTime == 0f && regenDelayTimer > 0f) ? "Waiting to Regen" :
            (currentMaskTime < maxMaskTime && currentMaskTime > 0f) ? "Mask Regenerating" :
            "Mask Full";

        Debug.Log($"{state}: {second}s");
    }
}
