using UnityEngine;
using UnityEngine.UI; 

public class HeartDisplay : MonoBehaviour
{
    public IntVariable playerHP;
    public Image[] hearts;      

    public void UpdateHearts()
    {
        // Loop through all 3 hearts
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < playerHP.Value)
            {
                hearts[i].enabled = true; 
            }
            else
            {
                hearts[i].enabled = false; 
            }
        }
    }
}