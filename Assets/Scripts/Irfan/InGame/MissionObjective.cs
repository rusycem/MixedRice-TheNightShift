using UnityEngine;
using TMPro;

public class MissionObjective : MonoBehaviour
{
    public IntVariable totalKeys;
    public TextMeshProUGUI missionText;

    // ADD THIS START FUNCTION
    void Start()
    {
        // This ensures the text appears immediately when you hit Play
        UpdateMissionStatus();
    }

    public void UpdateMissionStatus()
    {
        // Safety check to prevent errors if you forgot to drag the file
        if (totalKeys == null) return;

        if (totalKeys.Value >= totalKeys.MaxValue)
        {
            missionText.text = "Objective: ESCAPE THROUGH THE DOOR!";
            missionText.color = Color.green;
        }
        else
        {
            int remaining = totalKeys.MaxValue - totalKeys.Value;
            missionText.text = $"Objective: Find {remaining} more keys.";
        }
    }
}