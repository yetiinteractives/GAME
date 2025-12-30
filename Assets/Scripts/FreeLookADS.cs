using UnityEngine;
using Unity.Cinemachine;

public class FreeLookADS : MonoBehaviour
{
    public CinemachineCamera cam;

    public float normalFOV = 60f;
    public float adsFOV = 40f;
    public float fovLerpSpeed = 10f;

    [Header("Sensitivity Settings")]
    public float normalSensitivity = 1f;
    public float adsSensitivity = 0.5f;
    public float sensitivityLerpSpeed = 10f;

    private CinemachineInputAxisController _inputAxisController;
    private float _currentSensitivity = 1f;

    void Start()
    {
        // Get the input axis controller attached to the camera
        _inputAxisController = cam.GetComponent<CinemachineInputAxisController>();

        if (_inputAxisController == null)
        {
            Debug.LogWarning("No CinemachineInputAxisController found on the camera. Sensitivity adjustment will not work.");
        }
        else
        {
            _currentSensitivity = normalSensitivity;
        }
    }

    void Update()
    {
        bool aiming = Input.GetMouseButton(1);

        // --- FOV ---
        float targetFOV = aiming ? adsFOV : normalFOV;
        cam.Lens.FieldOfView = Mathf.Lerp(
            cam.Lens.FieldOfView,
            targetFOV,
            Time.deltaTime * fovLerpSpeed
        );

        // --- Sensitivity ---
        if (_inputAxisController != null)
        {
            float targetSensitivity = aiming ? adsSensitivity : normalSensitivity;
            _currentSensitivity = Mathf.Lerp(_currentSensitivity, targetSensitivity, Time.deltaTime * sensitivityLerpSpeed);

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
}