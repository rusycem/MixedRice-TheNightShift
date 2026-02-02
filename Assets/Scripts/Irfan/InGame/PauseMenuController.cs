using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Navigation")]
    [Tooltip("Drag the scene asset here. This ensures the reference persists even if you rename the file.")]
    [SerializeField] private Object mainMenuScene;

    public void ToggleVisuals()
    {
        // Check if time is stopped to determine if menu should show
        bool isPaused = Mathf.Approximately(Time.timeScale, 0f);
        gameObject.SetActive(isPaused);
    }

    public void LoadMainMenu()
    {
        if (mainMenuScene == null)
        {
            Debug.LogError($"[PauseMenuController] No Main Menu scene assigned on {gameObject.name}");
            return;
        }

        // Professional Reset: Ensure the engine is back to normal speed before switching
        Time.timeScale = 1f;

        // Use the name of the asset directly
        string sceneName = mainMenuScene.name;

        // Validation: Check if the scene is actually in the build list to avoid crashes
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"[PauseMenuController] Scene '{sceneName}' is not in the Build Settings! Please drag it into File > Build Settings.");
        }
    }
}