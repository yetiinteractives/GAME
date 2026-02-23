using JetBrains.Annotations;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class MousePosition3D : MonoBehaviour
{

    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask raycastLayerMask;
    [SerializeField] private float recoilBackDistance = 0.05f;
    [SerializeField] private float recoilUpPosition = 0.02f;
    [SerializeField] private float recoilUpDistance = 2f;
    [SerializeField] private float recoilKickSpeed = 18f;
    [SerializeField] private float recoilReturnSpeed = 12f;


    public static event Action<RaycastHit> OnFirePerformed;

    private Quaternion initialLocalRotation;
    private Coroutine recoilRoutine;
    private Vector3 recoilOffset;
    private Vector3 recoilTargetOffset;
    private Vector3 smoothedPosition;
  
    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetMouseButton(1))   //right click to aim
        {
            Transform hitTransform = null;
                if (mainCamera != null )
                {
            
                    // Raycast Check
                    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);  
                    if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, raycastLayerMask))
                    {
                    transform.position = Vector3.Lerp(transform.position, raycastHit.point, Time.deltaTime * smoothSpeed);
                    transform.position += recoilOffset;
                    hitTransform = raycastHit.transform;

                    if (Input.GetMouseButtonUp(0))  //left click 
                    {
                        if (hitTransform != null)
                        {
                            Debug.Log("Fire!!");
                            OnFirePerformed?.Invoke(raycastHit); //passing Transform of hit object
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
        Vector3 recoilBack = (mainCamera != null ? -mainCamera.transform.forward : -transform.forward) * recoilBackDistance;
        Vector3 recoilUp = (mainCamera != null ? mainCamera.transform.up : transform.up) * recoilUpPosition;
        recoilTargetOffset = recoilBack + recoilUp;

        if (recoilRoutine != null)
        {
            StopCoroutine(recoilRoutine);
        }

        recoilRoutine = StartCoroutine(ApplyRecoil());
    }

    private System.Collections.IEnumerator ApplyRecoil()
    {
        Quaternion recoilRotation = initialLocalRotation * Quaternion.Euler(-recoilUpDistance, 0f, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilKickSpeed;
            recoilOffset = Vector3.Lerp(Vector3.zero, recoilTargetOffset, t);
            transform.localRotation = Quaternion.Slerp(initialLocalRotation, recoilRotation, t);
            yield return null;          
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilReturnSpeed;
            recoilOffset = Vector3.Lerp(recoilTargetOffset, Vector3.zero, t);
            transform.localRotation = Quaternion.Slerp(recoilRotation, initialLocalRotation, t);
            yield return null;
        }

        recoilOffset = Vector3.zero;
        recoilRoutine = null;
    }
}
