using System;
using Unity.Cinemachine;
using UnityEngine;

public class Sniper : Weapon
{
    [SerializeField] private GameObject sniperScopeUI;
    [SerializeField] private FreeLookADS cameraController;

    [SerializeField] private GameObject playerBody;
    [SerializeField] private GameObject sniperBody;

    private bool isScopeActive = false;

    protected override void Awake()
    {
        base.Awake();

        if (sniperScopeUI != null)
        {
            sniperScopeUI.SetActive(false);
        }

        // Find camera controller if not assigned
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<FreeLookADS>();
            if (cameraController == null)
            {
                Debug.LogError("FreeLookADS controller not found in scene!");
            }
        }
    }

    protected override void ScopeCheck()
    {
        if (!isAiming)
        {
            if (isScopeActive)
            {
                DisableScope();
            }
            return;
        }

        // Weapon is aiming - check for scope toggle
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

    private void EnableScope()
    {
        if (sniperScopeUI == null || cameraController == null) return;

        isScopeActive = true;
        sniperScopeUI.SetActive(true);
        cameraController.SetScopedState();

       playerBody.SetActive(false);
       sniperBody.SetActive(false);
    }

    private void DisableScope()
    {
        if (sniperScopeUI == null || cameraController == null) return;

        isScopeActive = false;
        sniperScopeUI.SetActive(false);

        // Go back to ADS state (not fully normal)
        cameraController.SetADSState();

        playerBody.SetActive(true);
        sniperBody.SetActive(true);
    }

    protected override void Update()
    {
        base.Update();

        // If we're not aiming anymore but still have scope active, disable it
        if (!isAiming && isScopeActive)
        {
            DisableScope();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Reset when weapon is switched
        if (isScopeActive)
        {
            DisableScope();
        }

        // Let camera controller know we're done with ADS
        if (cameraController != null)
        {
            cameraController.SetNormalState();
        }
    }

    // Override base class methods to update camera controller
    protected override void StartAiming()
    {
        base.StartAiming();

        if (cameraController != null)
        {
            cameraController.SetADSState();
        }
    }

    protected override void StopAiming()
    {
        base.StopAiming();

        if (cameraController != null)
        {
            cameraController.SetNormalState();
        }
    }
}