using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("Events")]
    public GameEvent onPlayerDied;
    public GameEvent onTogglePause;

    [Header("Stamina Settings")]
    public float maxStamina = 5f;          
    public float staminaDrainRate = 1f;  
    public float staminaRegenRate = 0.5f; 
    public bool showDebugStamina = true;   

    [Header("Other")]
    public float gravityMultiplier = 2f;
    public MaskManager maskManager;
    public Slider staminaSlider;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;

    private float cameraPitch;
    private float verticalVelocity;

    private float currentStamina;

    private bool isPaused = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentStamina = maxStamina;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }

        // HARD RESET camera look to forward
        cameraPitch = 0f;

        if (cameraTarget != null)
        {
            cameraTarget.localEulerAngles = Vector3.zero;
        }
    }

    private void Update()
    {
        HandleMovement();
        ApplyLook();
        HandleStamina();
    }
    private void HandleStamina()
    {
        if (isRunning && moveInput.magnitude > 0 && !maskManager.isMaskOn)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isRunning = false;
                Debug.Log("Stamina depleted! Cannot sprint.");
            }
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina)
                currentStamina = maxStamina;
        }

        // Update the UI
        if (staminaSlider != null)
            staminaSlider.value = currentStamina;
    }
    private void HandleMovement()
    {
        if (!controller) return;

        bool maskActive = maskManager != null && maskManager.isMaskOn;
        float speed = (isRunning && !maskActive) ? runSpeed : walkSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move.Normalize();

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
        //cameraPitch -= pitch;
        //cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

        //cameraTarget.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
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

        if (maskManager != null && maskManager.isMaskOn)
        {
            if (value.isPressed)
                Debug.Log("Cannot sprint while wearing mask");

            isRunning = false;
            return;
        }

        isRunning = value.isPressed && currentStamina > 0f;
    }

    private void OnMask(InputValue value)
    {
        if (!value.isPressed) return;

        maskManager?.ToggleMask();

        if (maskManager != null && maskManager.isMaskOn)
            isRunning = false; // prevent running while mask is on
    }

    public void OnPause(InputValue value)
    {
        if (value.isPressed)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        // Use a ternary or simple if/else to ensure these ALWAYS fire together
        Time.timeScale = isPaused ? 0f : 1f;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;

        // This notifies your PauseMenuController
        onTogglePause?.Raise();
    }

    public void ResumePlay()
    {
        if (isPaused)
        {
            TogglePause();
            Debug.Log("Pressed");

        }
    }

    public void OnDie(InputValue value)
    {
        if (value.isPressed)
        {
            HandleDeath();
        }
    }

    public void HandleDeath()
    {
        onPlayerDied?.Raise();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Player dead!");

        this.enabled = false;
    }
}
