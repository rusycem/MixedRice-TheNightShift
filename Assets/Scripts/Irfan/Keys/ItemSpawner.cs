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
        // Loop through the list and spawn each specific key once
        foreach (GameObject keyPrefab in distinctKeys)
        {
            SpawnSingleKey(keyPrefab);
        }
    }

    void SpawnSingleKey(GameObject prefab)
    {
        // Try to find a valid point 10 times before giving up
        // (Prevents infinite loops if the map is tiny)
        for (int i = 0; i < 10; i++)
        {
            if (GetRandomPoint(out Vector3 spawnPoint))
            {
                Instantiate(prefab, spawnPoint, Quaternion.identity);
                return; // Success! Stop trying for this key.
            }
        }

        Debug.LogWarning($"Could not find a spot for {prefab.name}");
    }

    bool GetRandomPoint(out Vector3 result)
    {
        // 1. Pick a random X/Z within a sphere
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * spawnRadius;

        // 2. Ask NavMesh: "Is there valid BLUE ground near this random point?"
        NavMeshHit hit;

        // 2.0f is the search distance. NavMesh.AllAreas checks all walkble surfaces.
        if (NavMesh.SamplePosition(randomPoint, out hit, 2.0f, NavMesh.AllAreas))
        {
            // We found a spot! Adjust Y height so it doesn't clip the floor.
            result = hit.position + Vector3.up * spawnHeightOffset;
            return true;
        }

        result = Vector3.zero;
        return false;
    }
}