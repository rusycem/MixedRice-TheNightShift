using UnityEngine;
using UnityEngine.AI; // Required for NavMesh
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [Header("Items to Spawn")]
    // Drag your 4 Key Prefabs (Red, Blue, Green, Yellow) here
    public List<GameObject> distinctKeys;

    [Header("Settings")]
    public float spawnRadius = 20f; // How far to look for spots
    public float spawnHeightOffset = 0.5f; // Lift item slightly so it floats

    void Start()
    {
        SpawnAllKeys();
    }

    public void SpawnAllKeys()
    {
        int successfulSpawns = 0;

        // Loop through the list
        foreach (GameObject keyPrefab in distinctKeys)
        {
            // Try to spawn and capture the result (true/false)
            if (SpawnSingleKey(keyPrefab))
            {
                successfulSpawns++;
            }
        }

        Debug.Log($"SPAWN REPORT: Successfully spawned {successfulSpawns} / {distinctKeys.Count} keys.");
    }

    // Update SpawnSingleKey to return a 'bool' (true if spawned, false if failed)
    bool SpawnSingleKey(GameObject prefab)
    {
        for (int i = 0; i < 30; i++)
        {
            if (GetRandomPoint(out Vector3 spawnPoint))
            {
                Instantiate(prefab, spawnPoint, Quaternion.identity);
                return true; // Success!
            }
        }

        Debug.LogWarning($"FAILED to spawn: {prefab.name}");
        return false; // Failure
    }

    bool GetRandomPoint(out Vector3 result)
    {
        // 1. Pick a random X/Z within a sphere
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * spawnRadius;

        // 2. Ask NavMesh: "Is there valid BLUE ground near this random point?"
        NavMeshHit hit;

        // 10.0f is the search distance. NavMesh.AllAreas checks all walkble surfaces.
        if (NavMesh.SamplePosition(randomPoint, out hit, 10.0f, NavMesh.AllAreas))
        {
            // We found a spot! Adjust Y height so it doesn't clip the floor.
            result = hit.position + Vector3.up * spawnHeightOffset;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    // Add this to the bottom of ItemSpawner.cs
    void OnDrawGizmosSelected()
    {
        // Draw a Yellow sphere to show the spawn area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}