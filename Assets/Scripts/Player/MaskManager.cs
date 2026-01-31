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
    public GameObject maskVisual; 
    public float animationDuration = 0.5f; 
    public bool isMaskOn { get; private set; }

    private float currentMaskTime;
    private float regenDelayTimer = 0f;
    private bool maskEmpty = false;
    private bool isAnimating = false;

    private void Start()
    {
        currentMaskTime = maxMaskTime;
        if (maskVisual != null) //hide mask
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
            if (maskVisual != null)
                maskVisual.SetActive(true);

            maskAnimator.SetTrigger("OnMask");
            isMaskOn = true;
        }
        else
        {
            maskAnimator.SetTrigger("OffMask");
            isMaskOn = false;
        }

        Debug.Log(isMaskOn ? "Mask ON" : "Mask OFF");

        yield return new WaitForSeconds(animationDuration); //wait for anim

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
