using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player;
    public RectTransform playerIconUI; // Drag your UI Arrow here

    void LateUpdate()
    {
        if (player == null) return;
        // follow the player
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y; 
        transform.position = newPosition;

        // always face north
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        float playerRotation = player.eulerAngles.y;
        if (playerIconUI != null)
        {
            // Rotate the UI icon (Z-axis in UI corresponds to Y-axis in 3D)
            // Use -playerRotation if the icon spins the wrong way
            playerIconUI.localRotation = Quaternion.Euler(0, 0, -playerRotation);
        }
    }
}