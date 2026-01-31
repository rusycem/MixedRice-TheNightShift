using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public void ToggleVisuals()
    {
        // This is the logic: If On, turn Off. If Off, turn On.
        bool currentState = gameObject.activeSelf;
        gameObject.SetActive(!currentState);
    }
}