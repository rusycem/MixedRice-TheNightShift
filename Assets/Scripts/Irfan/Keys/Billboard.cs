using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            //find nearby camera
            mainCamera = Camera.allCameras.Length > 0 ? Camera.allCameras[0] : null;
            //Debug.LogWarning("Billboard couldn't find a 'MainCamera', using fallback.");
        }
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;
        // Rotate the object to face the camera
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                         mainCamera.transform.rotation * Vector3.up);
    }
}