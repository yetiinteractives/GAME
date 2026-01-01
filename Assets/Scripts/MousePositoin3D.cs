using System;
using UnityEngine;

public class MousePositoin3D : MonoBehaviour
{

    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask raycastLayerMask;


    public static event Action<RaycastHit> OnFirePerformed;
  
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
                    transform.position = raycastHit.point;
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

   
}
