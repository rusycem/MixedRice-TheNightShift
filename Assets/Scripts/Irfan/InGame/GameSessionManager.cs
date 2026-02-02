using UnityEngine;

public class GameSessionManager : MonoBehaviour
{
    [Header("Data to Reset")]
    public IntVariable keyCount;
    public IntVariable playerHealth;

    // Awake runs BEFORE any Start() function in the game
    void Awake()
    {
        ResetGameData();
    }

    public void ResetGameData()
    {
        // 1. Reset Keys
        keyCount.Value = 0;

        // 2. Reset Health
        playerHealth.Value = playerHealth.MaxValue;

        Debug.Log("Session Data Reset Complete.");
    }
}