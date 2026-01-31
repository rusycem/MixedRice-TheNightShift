using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        // follow the player
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y; 
        transform.position = newPosition;

        // always face north
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}