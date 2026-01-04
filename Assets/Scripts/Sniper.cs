using System;
using Unity.Cinemachine;
using UnityEngine;

public class Sniper : Weapon
{
    [SerializeField] private GameObject sniperScopeUI;
    [SerializeField] private FreeLookADS cameraController;

    // Body parts to hide when scoped 
    [SerializeField] private GameObject playerBody;
    [SerializeField] private GameObject sniperBody;
    [SerializeField] private GameObject sniperStock;

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

        // Ensure bodies are visible by default
        if (playerBody != null) playerBody.SetActive(true);
        if (sniperBody != null) sniperBody.SetActive(true);
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

        // Hide bodies when scoped
        if (playerBody != null) playerBody.SetActive(false);
        if (sniperBody != null) sniperBody.SetActive(false);
    }

    private void DisableScope()
    {
        if (sniperScopeUI == null || cameraController == null) return;

        isScopeActive = false;
        sniperScopeUI.SetActive(false);

        // Show bodies when not scoped
        if (playerBody != null) playerBody.SetActive(true);
        if (sniperBody != null) sniperBody.SetActive(true);

        // Only go to ADS state if we're still aiming
        if (isAiming)
        {
            cameraController.SetADSState();
        }
        else
        {
            cameraController.SetNormalState();
        }
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

        // Always reset to normal state when weapon is disabled
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

        sniperStock.SetActive(false); //avoid clipping
    }

    protected override void StopAiming()
    {
        base.StopAiming();

        // Disable scope first if it's active
        if (isScopeActive)
        {
            DisableScope();
        }
        else if (cameraController != null)
        {
            cameraController.SetNormalState();
        }

        sniperStock.SetActive(true); //restore stock visibility
    }
}