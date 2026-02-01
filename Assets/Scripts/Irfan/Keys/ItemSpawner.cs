using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [Header("Items to Spawn")]
    public List<GameObject> distinctKeys;

    [Header("Settings")]
    public float spawnRadius = 20f;
    public float spawnHeightOffset = 0.5f;
    [Tooltip("Items won't spawn if the spot is this far above/below the Spawner object")]
    public float maxVerticalDistance = 3f; // New setting to prevent ceiling spawns

    // LAYERS: We need to know what layer your floor is on (usually "Default")
    public LayerMask floorLayer = 1; // Default layer is usually 1, or set to 'Everything'

    [Header("Safety Checks")]
    public string wallTag = "Wall"; // TAG your walls with this!
    public float checkRadius = 0.5f; // Radius to check for walls around the item

    void Start()
    {
        SpawnAllKeys();
    }

    public void SpawnAllKeys()
    {
        int successfulSpawns = 0;

        foreach (GameObject keyPrefab in distinctKeys)
        {
            if (SpawnSingleKey(keyPrefab))
            {
                successfulSpawns++;
            }
        }

        Debug.Log($"SPAWN REPORT: Successfully spawned {successfulSpawns} / {distinctKeys.Count} keys.");
    }

    bool SpawnSingleKey(GameObject prefab)
    {
        // Increased attempts to 50 because we are being much stricter now
        for (int i = 0; i < 50; i++)
        {
            if (GetRandomPoint(out Vector3 spawnPoint))
            {
                Instantiate(prefab, spawnPoint, Quaternion.identity);
                return true;
            }
        }

        Debug.LogWarning($"FAILED to spawn: {prefab.name} (Map might be too crowded or radius too small)");
        return false;
    }

    bool GetRandomPoint(out Vector3 result)
    {
        result = Vector3.zero;

        // 1. Pick a random 2D point
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 randomPoint = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        NavMeshHit hit;

        // 2. Check NavMesh (Find a walkable spot nearby)
        // Note: We use a smaller range here (5.0f) to avoid grabbing ceilings if possible, but the height check later is the real fix.
        if (NavMesh.SamplePosition(randomPoint, out hit, 5.0f, NavMesh.AllAreas))
        {
            // 3. Raycast Check (The "Above Wall" Fix)
            // We fire a ray DOWN to find the surface.
            RaycastHit physicsHit;
            
            // Note: We check 'floorLayer' but walls might be on the same layer.
            if (Physics.Raycast(hit.position + Vector3.up * 5f, Vector3.down, out physicsHit, 10f, floorLayer))
            {
                // CHECK A: Did we land on top of a wall?
                if (physicsHit.collider.CompareTag(wallTag))
                {
                    return false; // Rejected: Landed on a wall
                }

                // CHECK B: Is the surface flat? (Walls are vertical, Floors are flat)
                // If the normal is too steep, it's probably a wall or a weird edge.
                if (Vector3.Angle(physicsHit.normal, Vector3.up) > 45f)
                {
                    return false; // Rejected: Surface is too steep (slope/wall)
                }

                Vector3 potentialPos = physicsHit.point + Vector3.up * spawnHeightOffset;

                // CHECK C: Height Check (The "Ceiling" Fix)
                // Only allow spawns that are roughly on the same level as this Spawner object.
                // If the spot is 10 units up (ceiling), this will reject it.
                if (Mathf.Abs(potentialPos.y - transform.position.y) > maxVerticalDistance)
                {
                    return false; // Rejected: Too high/low relative to Spawner
                }

                // CHECK D: Is the spot physically inside a wall? (The "Within Wall" Fix)
                // We create a small invisible ball at the spawn point and check if it touches any walls.
                Collider[] hits = Physics.OverlapSphere(potentialPos, checkRadius);
                foreach (Collider col in hits)
                {
                    if (col.CompareTag(wallTag))
                    {
                        return false; // Rejected: Too close to a wall
                    }
                }

                // If we survived all checks, it's a valid floor!
                result = potentialPos;
                return true;
            }
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, spawnRadius);
    }
}