using System;
using UnityEngine;

public class MousePositoin3D : MonoBehaviour
{

    [SerializeField] private Camera mainCamera;
    
    public static event Action<Transform , Transform> OnFirePerformed;
  
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
                    if (Physics.Raycast(ray, out RaycastHit raycastHit))
                    {
                    transform.position = raycastHit.point;
                    hitTransform = raycastHit.transform;
                    }
                }

            if (Input.GetMouseButtonUp(0))
            {
                if (hitTransform!= null)
                {
                    Debug.Log("Fire!!");
                    OnFirePerformed?.Invoke(hitTransform,transform); //passing Transform of hit object
                }
            }
        }
    }

   
}
