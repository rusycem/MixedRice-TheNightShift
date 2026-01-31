using UnityEngine;

[CreateAssetMenu(fileName = "IntVariable", menuName = "Scriptable Objects/IntVariable")]
public class IntVariable : ScriptableObject
{
    public int value;
    public int maxValue;

    public void ApplyChange(int amount)
    {
        value += amount;
        value = Mathf.Clamp(value, 0, maxValue);
    }
}