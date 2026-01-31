using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(fileName = "IntVariable", menuName = "Scriptable Objects/IntVariable")]
public class IntVariable : ScriptableObject
{
    public int Value;
    public int MaxValue;

    public void ApplyChange(int amount)
    {
        Value += amount;
        Value = Mathf.Clamp(Value, 0, MaxValue);

        //old ways for backup if clamp fail
        //if (Value > MaxValue) Value = MaxValue;
        //if (Value < 0) Value = 0;
    }
}