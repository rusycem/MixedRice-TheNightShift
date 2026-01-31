using UnityEngine;

public class HeadBobController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool _enable = true;
    [SerializeField] private Transform _camera = null;
    [SerializeField] private Transform _cameraHolder = null;

    [Header("Speeds")]
    [SerializeField] private float _walkThreshold = 2.0f;
    [SerializeField] private float _runThreshold = 7.0f;

    [Header("Idle (Breathing)")]
    [SerializeField] private float _idleAmp = 0.005f;
    [SerializeField] private float _idleFreq = 1.5f;

    [Header("Walking")]
    [SerializeField] private float _walkAmp = 0.02f;
    [SerializeField] private float _walkFreq = 12.0f;

    [Header("Running")]
    [SerializeField] private float _runAmp = 0.04f;
    [SerializeField] private float _runFreq = 16.0f;

    private Vector3 _startPos;
    private CharacterController _controller;
    private float _timer;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _startPos = _camera.localPosition;
    }

    void Update()
    {
        if (!_enable) return;

        CheckMotion();
        ResetPosition();
    }

    private void CheckMotion()
    {
        float speed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;

        if (!_controller.isGrounded) return;

        if (speed < _walkThreshold)
        {
            // Idle
            PlayBob(_idleFreq, _idleAmp);
        }
        else if (speed < _runThreshold)
        {
            // Walking
            PlayBob(_walkFreq, _walkAmp);
        }
        else
        {
            // Running
            PlayBob(_runFreq, _runAmp);
        }
    }

    private void PlayBob(float freq, float amp)
    {
        _timer += Time.deltaTime * freq;

        Vector3 newPos = _camera.localPosition;
        newPos.y += Mathf.Sin(_timer) * amp;
        newPos.x += Mathf.Cos(_timer / 2) * amp;

        _camera.localPosition = newPos;
    }

    private void ResetPosition()
    {
        _camera.localPosition = Vector3.Lerp(_camera.localPosition, _startPos, 1.0f * Time.deltaTime);
    }
}