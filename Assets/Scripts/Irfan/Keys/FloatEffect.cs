using UnityEngine;

public class FloatEffect : MonoBehaviour
{
    [Header("Bobbing Settings")]
    public float speed = 3f;      // How fast it moves up and down
    public float height = 0.25f;  // How high it moves

    private Vector3 startPos;

    void Start()
    {
        // Remember where we spawned so we bob relative to that spot
        startPos = transform.position;
    }

    void Update()
    {
        // Calculate the new Y position using a Sine wave (smooth up/down loop)
        float newY = startPos.y + Mathf.Sin(Time.time * speed) * height;

        // Update the position
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}