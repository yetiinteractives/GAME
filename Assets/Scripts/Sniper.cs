using System;
using UnityEngine;
using Unity.Cinemachine;


public class Sniper : Weapon
{
    [Header("Sniper Settings")]
    [SerializeField] private GameObject sniperScopeUI;
    [SerializeField] private FreeLookADS cameraController;

    [Header("Layer References")]
    [SerializeField] private LayerMask playerBodyLayer;
    [SerializeField] private GameObject sniperStock;

    
    public static event Action<bool> OnSniperStatusUpdate;

    
    private bool _isScopeActive = false;

    private bool _scopeKeyConsumed = false;

   
    private int _defaultMask;
    private int _scopedMask;

   
    protected override void Awake()
    {
        base.Awake();

        if (cameraController == null)
            cameraController = freeLookAds; 

        _defaultMask = Camera.main.cullingMask;
        _scopedMask = _defaultMask & ~playerBodyLayer.value;

        if (sniperScopeUI != null) sniperScopeUI.SetActive(false);
        if (sniperStock != null) sniperStock.SetActive(true);

        SetPlayerBodyVisible(true); // guarantee clean start
    }

    protected override void ScopeCheck()
    {
       
        if (Input.GetKeyUp(KeyCode.E))
        {
            _scopeKeyConsumed = false;
            return;
        }

        
        if (Input.GetKeyDown(KeyCode.E) && !_scopeKeyConsumed)
        {
            _scopeKeyConsumed = true;

            if (_isScopeActive) DisableScope();
            else EnableScope();
        }
    }

  
    protected override void StartAiming()
    {
        base.StartAiming();
        SetPlayerBodyVisible(true); 
    }

    protected override void StopAiming()
    {
      
        if (_isScopeActive)
            DisableScope();

        base.StopAiming(); 

        // ── FAILSAFE ─────────────────────────────────────────────────────
        // Unconditional hard restore on RMB release.
        // Runs last — overrides anything that glitched before it.
        SetPlayerBodyVisible(true);
       
    }

    private void EnableScope()
    {
        if (_isScopeActive) return;
        _isScopeActive = true;

        if (sniperScopeUI != null) sniperScopeUI.SetActive(true);
        if (sniperStock != null) sniperStock.SetActive(false);

        cameraController?.SetScopedState();
        SetPlayerBodyVisible(false); // hide

        OnSniperStatusUpdate?.Invoke(true);
    }

    private void DisableScope()
    {
        if (!_isScopeActive) return;
        _isScopeActive = false;
        _scopeKeyConsumed = false; 
        if (sniperScopeUI != null) sniperScopeUI.SetActive(false);
        if (sniperStock != null) sniperStock.SetActive(true);

      
        if (isAiming) cameraController?.SetADSState();
        else cameraController?.SetNormalState();

        SetPlayerBodyVisible(true); // restore

        OnSniperStatusUpdate?.Invoke(false);
    }

  
    protected override void OnDisable()
    {
        base.OnDisable(); 

        _isScopeActive = false;
        _scopeKeyConsumed = false;

        if (sniperScopeUI != null) sniperScopeUI.SetActive(false);
        if (sniperStock != null) sniperStock.SetActive(true);

        SetPlayerBodyVisible(true); // hard restore on disable
        cameraController?.SetNormalState();
        OnSniperStatusUpdate?.Invoke(false);
    }

    private void SetPlayerBodyVisible(bool visible)
    {
        // Layer 1: camera culling mask
        if (Camera.main != null)
            Camera.main.cullingMask = visible ? _defaultMask : _scopedMask;

       
    }
}