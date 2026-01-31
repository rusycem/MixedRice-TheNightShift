using UnityEngine;

public class MaskManager : MonoBehaviour
{
    [Header("Mask Settings")]
    public float maskDuration = 10f;
    public float maskCooldown = 5f;

    [Header("State")]
    public bool isMaskOn { get; private set; }
    public bool isOnCooldown { get; private set; }

    private float maskTimer = 0f;
    private float cooldownTimer = 0f;

    private void Update()
    {
        HandleMaskTimers();
    }

    private void HandleMaskTimers()
    {
        // Active mask countdown
        if (isMaskOn)
        {
            maskTimer -= Time.deltaTime;
            if (maskTimer <= 0f)
            {
                DisableMask();
                Debug.Log("Mask off");
            }
        }

        // Cooldown countdown
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                Debug.Log("Mask cd over");
            }
        }
    }

    public void ToggleMask()
    {
        if (isOnCooldown)
        {
            Debug.Log("Mask still in CD");
            return;
        }

        if (isMaskOn)
        {
            DisableMask();
        }
        else
        {
            EnableMask();
        }
    }

    private void EnableMask()
    {
        isMaskOn = true;
        maskTimer = maskDuration;
        Debug.Log("Mask ON");
    }

    private void DisableMask()
    {
        isMaskOn = false;
        isOnCooldown = true;
        cooldownTimer = maskCooldown;
        Debug.Log("Mask OFF. 5s CD started");
    }
}
