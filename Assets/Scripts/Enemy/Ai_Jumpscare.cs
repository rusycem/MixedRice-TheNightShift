using UnityEngine;
using System.Collections;

public class CameraJumpscare : MonoBehaviour
{
    private Quaternion originalRotation;
    public Transform aiMesh;
    public float returnSpeed = 5f;

    public void TriggerJumpscare(float duration)
    {
        StartCoroutine(JumpscareRoutine(duration));
    }

    IEnumerator JumpscareRoutine(float duration)
    {
        // 1. Save the EXACT rotation before the mess starts
        originalRotation = transform.rotation;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // 2. Force look at AI
            Vector3 direction = aiMesh.position - transform.position;
            transform.rotation = Quaternion.LookRotation(direction);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. Smoothly return to original rotation
        float returnElapsed = 0f;
        while (returnElapsed < 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, returnElapsed);
            returnElapsed += Time.deltaTime * returnSpeed;
            yield return null;
        }

        // 4. Final Snap to ensure 100% accuracy
        transform.rotation = originalRotation;
    }
}