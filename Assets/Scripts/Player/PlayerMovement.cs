using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public CharacterController controller;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public bool disableControl;

    [Header("Look Settings")]
    public Transform cameraTarget;
    public float lookSpeed = 5f;
    public float minPitch = -80f;
    public float maxPitch = 80f;
    public bool invertY = false;

    [Header("Gravity")]
    public float gravityMultiplier = 2f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;

    private float cameraPitch;
    private float verticalVelocity;

    private void Update()
    {
        HandleMovement();
        ApplyLook();
    }

    private void HandleMovement()
    {
        if (!controller) return;

        float speed = isRunning ? runSpeed : walkSpeed;

        // Movement direction
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move.Normalize();

        // Gravity
        if (controller.isGrounded)
            verticalVelocity = -2f;

        verticalVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;

        Vector3 velocity = move * speed + Vector3.up * verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    private void ApplyLook()
    {
        if (!cameraTarget) return;

        float yaw = lookInput.x * lookSpeed * Time.deltaTime;
        float pitch = lookInput.y * lookSpeed * Time.deltaTime;

        if (invertY)
            pitch = -pitch;

        transform.Rotate(Vector3.up * yaw);

        cameraPitch = Mathf.Clamp(cameraPitch - pitch, minPitch, maxPitch);
        cameraTarget.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void OnMove(InputValue value)
    {
        if (disableControl)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = value.Get<Vector2>();
    }

    private void OnLook(InputValue value)
    {
        if (disableControl)
        {
            lookInput = Vector2.zero;
            return;
        }

        lookInput = value.Get<Vector2>();
    }

    private void OnRun(InputValue value)
    {
        if (disableControl) return;

        isRunning = value.isPressed;
    }
}
