using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public void ToggleVisuals()
    {
        bool shouldBeActive = (Time.timeScale == 0f);
        gameObject.SetActive(shouldBeActive);
    }
}