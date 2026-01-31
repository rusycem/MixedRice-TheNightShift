using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Win Condition (Required)")]
    public IntVariable totalKeys;      // Drag the main 'KeyCount' (0/4) here
    public GameEvent onKeyCollected;   // Drag 'OnKeyCollected' here

    [Header("Inventory (Optional)")]
    public IntVariable specificKey;    // Drag 'RedKeyCount', 'BlueKeyCount' etc. here

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Add to the Global Total (For the Door/UI)
            totalKeys?.ApplyChange(1);

            // 2. Add to the Specific Color (For Inventory)
            // The '?' ensures it doesn't crash if you leave this empty!
            specificKey?.ApplyChange(1);

            // 3. Notify the System
            onKeyCollected?.Raise();

            Destroy(gameObject);
        }
    }
}