using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class Sniper : Weapon
{
    [SerializeField] private GameObject sniperScopeUI;
    private bool isScopeActive = false;

    [SerializeField] CinemachineCamera cinemachineCamera;
    [SerializeField]private float defaultFOV = 60f;
    [SerializeField]private float adsFOV = 40f;
    [SerializeField]private float scopedFOV = 20f;
    [SerializeField]private float fovTransitionSpeed = 10f;

    private float targetFOV;

    protected override void Awake()
    {
        base.Awake();
        if (sniperScopeUI != null)
        {
            sniperScopeUI.SetActive(false);
        }
    }

    protected override void  ScopeCheck() 
    {
        if (isAiming)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (isScopeActive)
                {
                    DisableScope();
                    
                }
                else
                {
                    EnableScope();
                }
            }
        }
        else
        {
            DisableScope();

        }
    }

    private void EnableScope()
    {
        isScopeActive = true;
        sniperScopeUI.SetActive(true);
       


    }

    private void DisableScope()
    {
        isScopeActive = false;
        sniperScopeUI.SetActive(false);
       
    }

    protected override void  Update()
    {
        base.Update();
        // Smoothly transition FOV
        if (isAiming)
        {
            targetFOV = isScopeActive ? scopedFOV : adsFOV;
        }
        else
        {
            targetFOV = defaultFOV; // Default FOV
        }
        cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(
            cinemachineCamera.Lens.FieldOfView,
            targetFOV,
            Time.deltaTime * fovTransitionSpeed
        );

    }

}
