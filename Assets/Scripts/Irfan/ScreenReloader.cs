using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenReloader : MonoBehaviour
{

    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(currentSceneName);
    }
}
