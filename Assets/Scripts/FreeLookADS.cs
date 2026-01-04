using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class FreeLookADS : MonoBehaviour
{
    public CinemachineCamera cam;

    [Header("Normal FOV Settings")]
    public float normalFOV = 60f;
    public float normalSensitivity = 1f;

    [Header("Weapon FOV Settings")]
    public float adsFOV = 40f;
    public float adsSensitivity = 0.5f;
    public float fovLerpSpeed = 10f;
    public float sensitivityLerpSpeed = 10f;

    [Header("Sniper Scope Settings")]
    public float scopedFOV = 20f;
    public float scopedSensitivity = 0.2f;
    public bool isScoped = false;
    public bool isADSActive = false;

    private CinemachineInputAxisController _inputAxisController;
    private float _currentSensitivity = 1f;
    private float _targetFOV;
    private float _targetSensitivity;

    void Start()
    {
        _inputAxisController = cam.GetComponent<CinemachineInputAxisController>();

        if (_inputAxisController == null)
        {
            Debug.LogWarning("No CinemachineInputAxisController found on the camera. Sensitivity adjustment will not work.");
        }
        else
        {
            _currentSensitivity = normalSensitivity;
            _targetSensitivity = normalSensitivity;
        }

        _targetFOV = normalFOV;
    }

    void Update()
    {
        // Update FOV
        cam.Lens.FieldOfView = Mathf.Lerp(
            cam.Lens.FieldOfView,
            _targetFOV,
            Time.deltaTime * fovLerpSpeed
        );

        // Update sensitivity
        if (_inputAxisController != null)
        {
            _currentSensitivity = Mathf.Lerp(_currentSensitivity, _targetSensitivity, Time.deltaTime * sensitivityLerpSpeed);

            // Apply sensitivity to all input axes
            for (int i = 0; i < _inputAxisController.Controllers.Count; i++)
            {
                var controller = _inputAxisController.Controllers[i];

                if (controller.Input != null)
                {
                    // Check if this is the Y axis (usually index 1)
                    // Invert sensitivity for Y axis to fix reversed vertical look
                    float gain = (i == 1) ? -_currentSensitivity : _currentSensitivity;
                    controller.Input.Gain = gain;
                }
            }
        }
    }

    // Public methods for weapons to call
    public void SetNormalState()
    {
        isScoped = false;
        isADSActive = false;
        _targetFOV = normalFOV;
        _targetSensitivity = normalSensitivity;
    }

    public void SetADSState()
    {
        isScoped = false;
        isADSActive = true;
        _targetFOV = adsFOV;
        _targetSensitivity = adsSensitivity;
    }

    public void SetScopedState()
    {
        isScoped = true;
        isADSActive = true;
        _targetFOV = scopedFOV;
        _targetSensitivity = scopedSensitivity;
    }

    public bool IsScoped()
    {
        return isScoped;
    }

    public bool IsADSActive()
    {
        return isADSActive;
    }
}