using UnityEngine;

public class DoorExit : MonoBehaviour
{
    [Header("Requirements")]
    public IntVariable totalKeys; // Drag 'KeyCount' here

    [Header("Visuals")]
    public GameObject doorObject; // The physical door mesh to hide/animate

    // Call this function via the GameEventListener!
    public void CheckExitCondition()
    {
        // Check if we have enough keys
        if (totalKeys.Value >= totalKeys.MaxValue)
        {
            UnlockDoor();
        }
        else
        {
            Debug.Log($"Door Locked: {totalKeys.Value} / {totalKeys.MaxValue}");
        }
    }

    private void UnlockDoor()
    {
        Debug.Log("DOOR UNLOCKED! YOU ESCAPE!");

        // Simple "Open" effect: Turn off the mesh and collider
        // (In the future, you can play an animation here)
        //gameObject.SetActive(false);
    }
}