using UnityEngine;
using UnityEngine.SceneManagement; // Needed if you want to load the cutscene scene directly

public class DoorExit : MonoBehaviour
{
    [Header("Requirements")]
    public IntVariable totalKeys; // ScriptableObject tracking current & max keys

    [Header("Visuals")]
    public SpriteRenderer doorRenderer; // Drag your sprite renderer here
    public Sprite lockedSprite;         // The default "Closed" sprite
    public Sprite unlockedSprite;       // The "Open" sprite shown when keys are collected

    [Header("Win Condition")]
    public GameEvent onLevelComplete;   // Trigger this event to play cutscene/win logic
    public string cutsceneSceneName;    // OR load this scene directly

    private bool isUnlocked = false;

    private void Start()
    {
        // Ensure the door looks locked at the start
        if (doorRenderer != null && lockedSprite != null)
        {
            doorRenderer.sprite = lockedSprite;
        }
    }

    // ---------------------------------------------------------
    // LINK THIS FUNCTION TO YOUR 'ON KEY COLLECTED' GAME EVENT LISTENER
    // ---------------------------------------------------------
    public void CheckExitCondition()
    {
        // Check if we have reached the max value defined in your IntVariable
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
        if (isUnlocked) return; // Prevent double unlocking

        isUnlocked = true;
        Debug.Log("DOOR UNLOCKED! ESCAPE ROUTE OPEN!");

        // 1. Change the sprite to the "Open" version
        if (doorRenderer != null && unlockedSprite != null)
        {
            doorRenderer.sprite = unlockedSprite;
        }
    }

    // 2. Win when the player touches the door
    private void OnTriggerEnter(Collider other)
    {
        // Only allow win if unlocked and it's the player
        if (isUnlocked && other.CompareTag("Player"))
        {
            TriggerWin();
        }
    }

    private void TriggerWin()
    {
        Debug.Log("Player exited the level! You Win!");

        // Option A: Raise the GameEvent (Best for decoupled logic)
        if (onLevelComplete != null)
        {
            onLevelComplete.Raise();
        }

        // Option B: Load the cutscene scene directly
        if (!string.IsNullOrEmpty(cutsceneSceneName))
        {
            SceneManager.LoadScene(cutsceneSceneName);
        }
    }
}