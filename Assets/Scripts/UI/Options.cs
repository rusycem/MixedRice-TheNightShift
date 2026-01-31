using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider volumeSlider;

    private const string VolumeKey = "mastervolume";

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1.0f);
        
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(delegate { SetVolume(); });
        }

        AudioListener.volume = savedVolume;
    }

    public void SetVolume()
    {
        float volumeValue = volumeSlider.value;
        
        AudioListener.volume = volumeValue;
        
        PlayerPrefs.SetFloat(VolumeKey, volumeValue);
        PlayerPrefs.Save();
        
        Debug.Log($"Volume set to: {volumeValue * 100}%");
    }
}
