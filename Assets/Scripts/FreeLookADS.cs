using UnityEngine;
using Unity.Cinemachine;

public class FreeLookADS : MonoBehaviour
{
    public CinemachineCamera cam;

    public float normalFOV = 60f;
    public float adsFOV = 40f;
    public float speed = 10f;

    void Update()
    {
        bool aiming = Input.GetMouseButton(1);

        float targetFOV = aiming ? adsFOV : normalFOV;

        cam.Lens.FieldOfView = Mathf.Lerp(
            cam.Lens.FieldOfView,
            targetFOV,
            Time.deltaTime * speed
        );
    }
}
