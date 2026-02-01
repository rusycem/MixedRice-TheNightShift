using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorExit : MonoBehaviour
{
    [Header("Requirements")]
    public IntVariable totalKeys;

    [Header("Visuals - 3D (Mesh)")]
    public MeshRenderer statusQuad; // The light/screen that turns Green
    public Material lockedMat;      // Red
    public Material unlockedMat;    // Green

    [Header("Visuals - 2D (Sprite)")]
    public SpriteRenderer doorSpriteRenderer; // Optional: For sprite-based doors
    public Sprite lockedSprite;     // Sprite for closed state
    public Sprite unlockedSprite;   // Sprite for open state

    [Header("Physics & Win")]
    public Collider doorCollider;     // The box collider blocking the path
    public GameEvent onLevelComplete; // (Optional) Event to trigger UI/Sounds
    public string cutsceneSceneName;  // (Optional) Scene to load

    // Internal state to track if we are open
    private bool isUnlocked = false;

    void Start()
    {
        // Quality of Life: Try to find components automatically
        if (statusQuad == null) statusQuad = GetComponentInChildren<MeshRenderer>();
        if (doorSpriteRenderer == null) doorSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (doorCollider == null) doorCollider = GetComponent<Collider>();

        // Initialize state
        UpdateDoorState();
    }

    // ---------------------------------------------------------
    // LINK THIS FUNCTION TO YOUR 'ON KEY COLLECTED' GAME EVENT LISTENER
    // ---------------------------------------------------------
    public void CheckExitCondition()
    {
        UpdateDoorState();
    }

    private void UpdateDoorState()
    {
        if (totalKeys == null) return;

        // Check if we have enough keys
        bool conditionMet = totalKeys.Value >= totalKeys.MaxValue;

        if (conditionMet)
        {
            UnlockDoor();
        }
        else
        {
            LockDoor();
        }
    }

    private void LockDoor()
    {
        isUnlocked = false;

        // Visuals: Red Material (3D)
        if (statusQuad && lockedMat)
            statusQuad.material = lockedMat;

        // Visuals: Closed Sprite (2D)
        if (doorSpriteRenderer && lockedSprite)
            doorSpriteRenderer.sprite = lockedSprite;

        // Physics: Solid Wall (Not a trigger)
        if (doorCollider)
        {
            doorCollider.enabled = true;
            doorCollider.isTrigger = false;
        }
    }

    private void UnlockDoor()
    {
        if (isUnlocked) return; // Don't run this twice

        isUnlocked = true;
        Debug.Log("DOOR UNLOCKED! ESCAPE ROUTE OPEN!");

        // Visuals: Green Material (3D)
        if (statusQuad && unlockedMat)
            statusQuad.material = unlockedMat;

        // Visuals: Open Sprite (2D)
        if (doorSpriteRenderer && unlockedSprite)
            doorSpriteRenderer.sprite = unlockedSprite;

        // Physics: Walk-through Trigger (Allows winning)
        if (doorCollider)
        {
            doorCollider.enabled = true;
            doorCollider.isTrigger = true;
        }
    }

    // ---------------------------------------------------------
    // WIN LOGIC (Runs when player walks into the open door)
    // ---------------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        // Only trigger if unlocked AND it's the player
        if (isUnlocked && other.CompareTag("Player"))
        {
            TriggerWin();
        }
    }

    private void TriggerWin()
    {
        Debug.Log("Player exited the level! You Win!");

        // Option A: Raise Game Event
        if (onLevelComplete != null)
        {
            onLevelComplete.Raise();
        }

        // Option B: Load Cutscene/Menu
        if (!string.IsNullOrEmpty(cutsceneSceneName))
        {
            SceneManager.LoadScene(cutsceneSceneName);
        }
    }
}