using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Future Data (Optional for now)")]
    // The ? lets us check if it exists before trying to use it.
    public GameEvent onItemPickedUp;
    public IntVariable itemCounter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Run Placeholder Logic (Safe Mode)
            // The '?' means: "Only do this if the slot is NOT empty"
            itemCounter?.ApplyChange(1);
            onItemPickedUp?.Raise();

            // 2. Debug so you know it worked
            Debug.Log("Item Collected! (System ready for data)");

            // 3. Destroy visual
            Destroy(gameObject);
        }
    }
}