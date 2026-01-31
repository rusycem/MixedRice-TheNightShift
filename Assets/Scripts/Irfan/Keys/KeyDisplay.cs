using UnityEngine;
using TMPro; // Required for TextMeshPro

public class KeyDisplay : MonoBehaviour
{
    public IntVariable keyCount; // Drag 'KeyCount' asset here
    public TextMeshProUGUI keyText; // Drag this object (the text component) here

    void Start()
    {
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        // Formats it as "Current / Max" (e.g., "1 / 4")
        keyText.text = $"{keyCount.Value} / {keyCount.MaxValue}";
    }
}