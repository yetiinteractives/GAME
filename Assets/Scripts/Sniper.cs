using System;
using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Sniper rifle — extends Weapon.
///
/// Aim:   inherited from Weapon (GameInput.AimDown / AimUp → StartAiming / StopAiming)
/// Scope: ScopeCheck() override — Input.GetKeyDown(E) tap only, guarded against hold.
///
/// Visibility uses TWO layers of protection:
///   1. Camera.cullingMask  — strips the layer from rendering
///   2. SkinnedMeshRenderer.enabled — disables renderer directly
/// This way SetActive() calls on flashlight/children can never accidentally restore visibility.
///
/// FAILSAFE: StopAiming() force-restores player visibility unconditionally on RMB release.
/// </summary>
public class Sniper : Weapon
{
    [Header("Sniper Settings")]
    [SerializeField] private GameObject sniperScopeUI;
    [SerializeField] private FreeLookADS cameraController;

    [Header("Layer References")]
    [SerializeField] private LayerMask playerBodyLayer;
    [SerializeField] private GameObject sniperStock;

    [Header("Player Body Renderers")]
    [Tooltip("Drag every SkinnedMeshRenderer on the player body here. These get disabled when scoped so SetActive() calls on children can't restore visibility.")]
    [SerializeField] private SkinnedMeshRenderer[] playerBodyRenderers;

    // ── Public event for HUD / other systems ─────────────────────────────
    public static event Action<bool> OnSniperStatusUpdate;

    // ── Internal state ────────────────────────────────────────────────────
    private bool _isScopeActive = false;

    /// <summary>
    /// Consumed on first GetKeyDown, cleared only on GetKeyUp.
    /// Guarantees exactly one toggle per physical press no matter how long E is held.
    /// </summary>
    private bool _scopeKeyConsumed = false;

    // ── Culling masks, computed once ──────────────────────────────────────
    private int _defaultMask;
    private int _scopedMask;

    // ─────────────────────────────────────────────────────────────────────
    protected override void Awake()
    {
        base.Awake();

        if (cameraController == null)
            cameraController = freeLookAds; // reuse what Weapon.Awake() already found

        _defaultMask = Camera.main.cullingMask;
        _scopedMask = _defaultMask & ~playerBodyLayer.value;

        if (sniperScopeUI != null) sniperScopeUI.SetActive(false);
        if (sniperStock != null) sniperStock.SetActive(true);

        SetPlayerBodyVisible(true); // guarantee clean start
    }

    // ─────────────────────────────────────────────────────────────────────
    // Called by Weapon.Update() every frame while isAiming is true
    // ─────────────────────────────────────────────────────────────────────
    protected override void ScopeCheck()
    {
        // ── Release always clears the lock first ──
        if (Input.GetKeyUp(KeyCode.E))
        {
            _scopeKeyConsumed = false;
            return;
        }

        // ── One toggle per tap — hold does nothing after first frame ──
        if (Input.GetKeyDown(KeyCode.E) && !_scopeKeyConsumed)
        {
            _scopeKeyConsumed = true;

            if (_isScopeActive) DisableScope();
            else EnableScope();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Aim overrides
    // ─────────────────────────────────────────────────────────────────────
    protected override void StartAiming()
    {
        base.StartAiming(); // sets isAiming = true, calls freeLookAds.SetADSState()
        SetPlayerBodyVisible(true); // not scoped yet — body visible
    }

    protected override void StopAiming()
    {
        // Kill scope before aim dies
        if (_isScopeActive)
            DisableScope();

        base.StopAiming(); // sets isAiming = false, calls freeLookAds.SetNormalState()

        // ── FAILSAFE ─────────────────────────────────────────────────────
        // Unconditional hard restore on RMB release.
        // Runs last — overrides anything that glitched before it.
        SetPlayerBodyVisible(true);
        // ─────────────────────────────────────────────────────────────────
    }

    // ─────────────────────────────────────────────────────────────────────
    // Scope enable / disable
    // ─────────────────────────────────────────────────────────────────────
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
        _scopeKeyConsumed = false; // reset lock so next aim-in works cleanly

        if (sniperScopeUI != null) sniperScopeUI.SetActive(false);
        if (sniperStock != null) sniperStock.SetActive(true);

        // isAiming still true here when called from StopAiming (before base call)
        if (isAiming) cameraController?.SetADSState();
        else cameraController?.SetNormalState();

        SetPlayerBodyVisible(true); // restore

        OnSniperStatusUpdate?.Invoke(false);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Weapon swapped / disabled mid-scope — hard reset
    // ─────────────────────────────────────────────────────────────────────
    protected override void OnDisable()
    {
        base.OnDisable(); // parent unsubscribes fire event, stops reload coroutine

        _isScopeActive = false;
        _scopeKeyConsumed = false;

        if (sniperScopeUI != null) sniperScopeUI.SetActive(false);
        if (sniperStock != null) sniperStock.SetActive(true);

        SetPlayerBodyVisible(true); // hard restore on disable
        cameraController?.SetNormalState();
        OnSniperStatusUpdate?.Invoke(false);
    }

    // ─────────────────────────────────────────────────────────────────────
    // TWO-LAYER visibility — cullingMask + renderer.enabled
    // Nothing outside this method should touch either of these
    // ─────────────────────────────────────────────────────────────────────
    private void SetPlayerBodyVisible(bool visible)
    {
        // Layer 1: camera culling mask
        if (Camera.main != null)
            Camera.main.cullingMask = visible ? _defaultMask : _scopedMask;

        // Layer 2: disable the renderers directly
        // SetActive() on ANY child/sibling cannot restore these once disabled
        if (playerBodyRenderers == null) return;
        foreach (var r in playerBodyRenderers)
        {
            if (r != null) r.enabled = visible;
        }
    }
}