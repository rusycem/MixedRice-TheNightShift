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

    // LAYERS: We need to know what layer your floor is on (usually "Default")
    public LayerMask floorLayer = 1; // Default layer is usually 1, or set to 'Everything'

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
        for (int i = 0; i < 30; i++)
        {
            if (GetRandomPoint(out Vector3 spawnPoint))
            {
                Instantiate(prefab, spawnPoint, Quaternion.identity);
                return true;
            }
        }

        Debug.LogWarning($"FAILED to spawn: {prefab.name}");
        return false;
    }

    bool GetRandomPoint(out Vector3 result)
    {
        // FIX 1: Use insideUnitCircle (2D Flat) instead of Sphere (3D Ball)
        // This prevents picking points high in the air near the ceiling
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

        // Map the 2D circle to 3D space (X, 0, Z) relative to the spawner
        Vector3 randomPoint = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        NavMeshHit hit;

        // FIX 2: Check NavMesh
        if (NavMesh.SamplePosition(randomPoint, out hit, 10.0f, NavMesh.AllAreas))
        {
            // FIX 3: The "Physics Check" (Raycast)
            // Even if NavMesh says "OK", let's double-check where the visual floor is.
            // We fire a ray from 2 units ABOVE the NavMesh point, downwards.

            RaycastHit physicsHit;
            if (Physics.Raycast(hit.position + Vector3.up * 2f, Vector3.down, out physicsHit, 5f, floorLayer))
            {
                // We hit the actual floor mesh! Spawn relative to THAT, not the NavMesh.
                result = physicsHit.point + Vector3.up * spawnHeightOffset;
                return true;
            }

            // Fallback: If raycast failed (rare), just use NavMesh point
            result = hit.position + Vector3.up * spawnHeightOffset;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // Draw a flat disc to visualize the new search area
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, spawnRadius);
    }
}