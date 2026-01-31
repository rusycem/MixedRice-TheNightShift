using UnityEngine;
using UnityEngine.AI; // Required for finding valid ground

public class ItemSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject itemPrefab; // Drag your 'FloatingKey' prefab here
    public float spawnRadius = 10f; // How far to look for a spot
    public int spawnCount = 3;

    void Start()
    {
        SpawnItems();
    }

    public void SpawnItems()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 randomPoint = GetRandomPointOnNavMesh();

            // Spawn slightly above ground (y + 0.5) so sprite doesn't clip floor
            Instantiate(itemPrefab, randomPoint + Vector3.up * 0.5f, Quaternion.identity);
        }
    }

    Vector3 GetRandomPointOnNavMesh()
    {
        // 1. Pick a random point in a sphere
        Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
        randomDirection += transform.position;

        // 2. Ask NavMesh: "Is there valid ground near this point?"
        NavMeshHit hit;

        // 1.0f is the max distance to snap to the floor. 
        // NavMesh.AllAreas allows it to spawn anywhere walkable.
        if (NavMesh.SamplePosition(randomDirection, out hit, 5.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // If we missed the NavMesh, just return the spawner's position as a fallback
        return transform.position;
    }
}