using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-200)]
public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private InputActionMap actionMap;
    private InputAction aimAction;
    private InputAction fireAction;
    private InputAction reloadAction;
    private InputAction weaponWheelAction;
    private InputAction flashlightAction;
    private InputAction scopeToggleAction;

    // ── Public per-frame state ──────────────────────────────────────
    public static bool Aim { get; private set; }
    public static bool AimDown { get; private set; }
    public static bool AimUp { get; private set; }

    public static bool Fire { get; private set; }
    public static bool FireDown { get; private set; }
    public static bool FireUp { get; private set; }

    public static bool ReloadDown { get; private set; }
    public static bool WeaponWheelDown { get; private set; }
    public static bool WeaponWheelUp { get; private set; }
    public static bool FlashlightDown { get; private set; }

    // ScopeToggleDown kept for any other scripts that still poll it
    public static bool ScopeToggleDown { get; private set; }

    // ── NEW: event fired once on the leading edge of E / rightStickPress ──
    public static event Action OnScopeToggle;

    // ── Auto-bootstrap ──────��──────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreate()
    {
        if (Instance != null) return;
        var go = new GameObject("[GameInput]");
        go.AddComponent<GameInput>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildActions();
        actionMap.Enable();
    }

    void BuildActions()
    {
        actionMap = new InputActionMap("Player");

        aimAction = actionMap.AddAction("Aim", type: InputActionType.Button);
        aimAction.AddBinding("<Mouse>/rightButton");
        aimAction.AddBinding("<Gamepad>/leftTrigger");

        fireAction = actionMap.AddAction("Fire", type: InputActionType.Button);
        fireAction.AddBinding("<Mouse>/leftButton");
        fireAction.AddBinding("<Gamepad>/rightTrigger");

        reloadAction = actionMap.AddAction("Reload", type: InputActionType.Button);
        reloadAction.AddBinding("<Keyboard>/r");
        reloadAction.AddBinding("<Gamepad>/buttonWest");

        weaponWheelAction = actionMap.AddAction("WeaponWheel", type: InputActionType.Button);
        weaponWheelAction.AddBinding("<Keyboard>/tab");
        weaponWheelAction.AddBinding("<Gamepad>/select");

        flashlightAction = actionMap.AddAction("Flashlight", type: InputActionType.Button);
        flashlightAction.AddBinding("<Keyboard>/t");
        flashlightAction.AddBinding("<Gamepad>/rightShoulder");

        scopeToggleAction = actionMap.AddAction("ScopeToggle", type: InputActionType.Button);
        scopeToggleAction.AddBinding("<Keyboard>/e");
        scopeToggleAction.AddBinding("<Gamepad>/rightStickPress");

        // ── Scope: fire event on leading edge only (tap, not hold/release) ──
        scopeToggleAction.started += _ => OnScopeToggle?.Invoke();
    }

    void Update()
    {
        // ── Aim ──
        Aim = aimAction.IsPressed();
        AimDown = aimAction.WasPressedThisFrame();
        AimUp = aimAction.WasReleasedThisFrame();

        // ── Fire ──
        Fire = fireAction.IsPressed();
        FireDown = fireAction.WasPressedThisFrame();
        FireUp = fireAction.WasReleasedThisFrame();

        // ── Buttons ──
        ReloadDown = reloadAction.WasPressedThisFrame();
        WeaponWheelDown = weaponWheelAction.WasPressedThisFrame();
        WeaponWheelUp = weaponWheelAction.WasReleasedThisFrame();
        FlashlightDown = flashlightAction.WasPressedThisFrame();

        // ScopeToggleDown still updated for any legacy polling consumers
        ScopeToggleDown = scopeToggleAction.WasPressedThisFrame();
    }

    void OnEnable() => actionMap?.Enable();
    void OnDisable() => actionMap?.Disable();

    void OnDestroy()
    {
        actionMap?.Disable();
        actionMap?.Dispose();
        if (Instance == this) Instance = null;
    }

    // ── Reset all states on death ──────────────────────────────────
    public void OnPlayerDeath()
    {
        actionMap?.Disable();
        ResetAllStates();
    }

    // ── Reset all states on respawn ────────────────────────────────
    public void OnPlayerRespawn()
    {
        actionMap?.Enable();
        ResetAllStates();
    }

    private void ResetAllStates()
    {
        Aim = AimDown = AimUp = false;
        Fire = FireDown = FireUp = false;
        ReloadDown = false;
        WeaponWheelDown = WeaponWheelUp = false;
        FlashlightDown = false;
        ScopeToggleDown = false;
    }
}