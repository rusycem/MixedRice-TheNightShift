using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    [Header("Data")]
    public IntVariable playerHP; //int variabble SO

    [Header("Events")]
    public GameEvent onHealthChanged; 
    public GameEvent onPlayerDied;   

    void Start()
    {
        // reset hp
        playerHP.Value = playerHP.MaxValue;
        onHealthChanged.Raise();
    }

    public void OnTestHitMe(InputValue value)
    {
        if (value.isPressed)
        {
            TakeDamage(1);
        }
    }


    public void TakeDamage(int damage)
    {
        playerHP.ApplyChange(-damage);
        onHealthChanged.Raise(); 

        if (playerHP.Value <= 0)
        {
            onPlayerDied.Raise();
            //Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}