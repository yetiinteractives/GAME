using System;
using System.Collections;
using UnityEngine;

public class MousePosition3D : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask raycastLayerMask;

    [Header("Recoil Position")]
    [SerializeField] private float recoilBackDistance = 0.05f;
    [SerializeField] private float recoilUpPosition = 0.02f;

    [Header("Recoil Rotation")]
    [SerializeField] private float recoilUpDistance = 2f;

    [Header("Recoil Speed")]
    [SerializeField] private float recoilKickSpeed = 18f;
    [SerializeField] private float recoilReturnSpeed = 12f;

    [Header("Recoil Distance Scaling")]
    [SerializeField] private Transform recoilDistanceOrigin;
    [SerializeField] private float recoilMinDistance = 2f;
    [SerializeField] private float recoilMaxDistance = 50f;
    [SerializeField] private float recoilMinScale = 0.4f;
    [SerializeField] private float recoilMaxScale = 1.2f;

    public static event Action<RaycastHit> OnFirePerformed;

    private Quaternion initialLocalRotation;
    private Coroutine recoilRoutine;
    private Vector3 recoilOffset;
    private Vector3 recoilTargetOffset;
    private Vector3 smoothedPosition;
    private float lastHitDistance;

    void Update()
    {
        if (GameInput.Aim) // right click / left trigger to aim
        {
            if (mainCamera != null)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                // QueryTriggerInteraction.Collide lets us hit trigger colliders
                // on alive enemies AND regular colliders on dead ragdolls
                if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity,
                    raycastLayerMask, QueryTriggerInteraction.Collide))
                {
                    Transform origin = recoilDistanceOrigin != null
                        ? recoilDistanceOrigin : transform;
                    lastHitDistance = Vector3.Distance(origin.position, raycastHit.point);

                    transform.position = Vector3.Lerp(
                        transform.position, raycastHit.point,
                        Time.deltaTime * smoothSpeed);
                    transform.position += recoilOffset;

                    if (GameInput.FireUp) // left click release / right trigger release to fire
                    {
                        if (raycastHit.transform != null)
                        {
                            Debug.Log("Fire!!");
                            OnFirePerformed?.Invoke(raycastHit);
                        }
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        smoothedPosition = transform.position;
        Weapon.OnBulletShot += HandRecoil;
    }

    private void OnDisable()
    {
        Weapon.OnBulletShot -= HandRecoil;
    }

    private void HandRecoil()
    {
        initialLocalRotation = transform.localRotation;

        float distanceT = Mathf.InverseLerp(recoilMinDistance, recoilMaxDistance, lastHitDistance);
        float recoilScale = Mathf.Lerp(recoilMinScale, recoilMaxScale, distanceT);

        Vector3 camForward = mainCamera != null ? mainCamera.transform.forward : transform.forward;
        Vector3 camUp = mainCamera != null ? mainCamera.transform.up : transform.up;

        Vector3 recoilBack = -camForward * recoilBackDistance;
        Vector3 recoilUp = camUp * recoilUpPosition;
        recoilTargetOffset = (recoilBack + recoilUp) * recoilScale;

        if (recoilRoutine != null)
            StopCoroutine(recoilRoutine);

        recoilRoutine = StartCoroutine(ApplyRecoil(recoilScale));
    }

    private IEnumerator ApplyRecoil(float recoilScale)
    {
        Quaternion recoilRotation = initialLocalRotation
            * Quaternion.Euler(-recoilUpDistance * recoilScale, 0f, 0f);

        // Kick phase
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilKickSpeed;
            recoilOffset = Vector3.Lerp(Vector3.zero, recoilTargetOffset, t);
            transform.localRotation = Quaternion.Slerp(
                initialLocalRotation, recoilRotation, t);
            yield return null;
        }

        // Return phase
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilReturnSpeed;
            recoilOffset = Vector3.Lerp(recoilTargetOffset, Vector3.zero, t);
            transform.localRotation = Quaternion.Slerp(
                recoilRotation, initialLocalRotation, t);
            yield return null;
        }

        recoilOffset = Vector3.zero;
        recoilRoutine = null;
    }
}