using System;
using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Sniper rifle.
/// 
/// Scope input is 100% self-contained — no GameInput, no New Input System.
/// Uses Input.GetKeyDown (tap only — one fire per physical press, ignores hold/release).
/// 
/// Visibility rule (single source of truth — ApplyCullingMask):
///   Hide player body  =  isAiming AND isScopeActive
///   Show player body  =  everything else
/// </summary>
public class Sniper : Weapon
{
    [Header("Sniper Settings")]
    [SerializeField] private GameObject sniperScopeUI;
    [SerializeField] private FreeLookADS cameraController;

    [Header("Layer References")]
    [SerializeField] private LayerMask playerBodyLayer;
    [SerializeField] private GameObject sniperStock;

    [Header("Scope Key")]
    [SerializeField] private KeyCode scopeKey = KeyCode.E;

    // ── Public event for HUD / other systems ─────────────────────────────
    public static event Action<bool> OnSniperStatusUpdate;

    // ── Internal state ────────────────────────────────────────────────────
    private bool _isScopeActive = false;

    // ── Culling masks, computed once in Awake ─────────────────────────────
    private int _defaultMask;
    private int _scopedMask;

    // ─────────────────────────────────────────────────────────���───────────
    protected override void Awake()
    {
        base.Awake();

        if (cameraController == null)
            cameraController = FindFirstObjectByType<FreeLookADS>();

        _defaultMask = Camera.main.cullingMask;
        _scopedMask = _defaultMask & ~playerBodyLayer.value;

        // Clean initial state
        if (sniperScopeUI != null) sniperScopeUI.SetActive(false);
        if (sniperStock != null) sniperStock.SetActive(true);
        ApplyCullingMask(hideBody: false);
    }

    // ─────────────────────────────────────────────────────────────────────
    protected override void Update()
    {
        base.Update();

        // TAP only — Input.GetKeyDown is true for exactly one frame per press.
        // Hold and release are completely ignored.
        if (Input.GetKeyDown(scopeKey))
        {
            // Silently ignore if not aiming — no state mutation at all
            if (isAiming)
            {
                if (_isScopeActive) DisableScope();
                else EnableScope();
            }
        }

        // Failsafe: if aim was dropped while scope was on,
        // kill scope. This handles any edge case where StopAiming
        // didn't fire (e.g. weapon swap mid-aim, death, etc.)
        if (_isScopeActive && !isAiming)
            DisableScope();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Base-class aim overrides
    // ─────────────────────────────────────────────────────────────────────
    protected override void StartAiming()
    {
        base.StartAiming();
        cameraController?.SetADSState();
        ApplyCullingMask(hideBody: false); // Not scoped yet — body visible
    }

    protected override void StopAiming()
    {
        base.StopAiming();

        if (_isScopeActive)
            DisableScope();             // Handles mask + camera restore
        else
        {
            ApplyCullingMask(hideBody: false);
            cameraController?.SetNormalState();
        }
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
        ApplyCullingMask(hideBody: true);

        OnSniperStatusUpdate?.Invoke(true);
    }

    private void DisableScope()
    {
        if (!_isScopeActive) return;
        _isScopeActive = false;

        if (sniperScopeUI != null) sniperScopeUI.SetActive(false);
        if (sniperStock != null) sniperStock.SetActive(true);

        // Restore camera to the right state for current aim
        if (isAiming) cameraController?.SetADSState();
        else cameraController?.SetNormalState();

        ApplyCullingMask(hideBody: false);

        OnSniperStatusUpdate?.Invoke(false);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Called when weapon is swapped away / object disabled mid-scope
    // ─────────────────────────────────────────────────────────────────────
    protected override void OnDisable()
    {
        base.OnDisable();

        _isScopeActive = false;

        if (sniperScopeUI != null) sniperScopeUI.SetActive(false);
        if (sniperStock != null) sniperStock.SetActive(true);

        if (Camera.main != null) Camera.main.cullingMask = _defaultMask;

        cameraController?.SetNormalState();
        OnSniperStatusUpdate?.Invoke(false);
    }

    // ─────────────────────────────────────────────────────────────────────
    // THE single culling-mask write point — nowhere else touches this
    // ─────────────────────────────────────────────────────────────────────
    private void ApplyCullingMask(bool hideBody)
    {
        if (Camera.main == null) return;
        Camera.main.cullingMask = hideBody ? _scopedMask : _defaultMask;
    }
}