using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Centralised input wrapper built on Unity's New Input System.
/// Supports keyboard+mouse and gamepad simultaneously.
/// Auto-creates itself — no manual scene setup required.
///
/// KB+M bindings are identical to the originals:
///   RMB → Aim   LMB → Fire   R → Reload   Tab → Weapon Wheel
///   T → Flashlight   E → Scope Toggle
///
/// Gamepad bindings (Xbox / PlayStation):
///   LT / L2  → Aim           RT / R2 → Fire
///   X / Square → Reload      Select / Touchpad → Weapon Wheel
///   RB / R1 → Flashlight     R-Stick Click / R3 → Scope Toggle
/// </summary>
[DefaultExecutionOrder(-200)]
public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    // ── InputActions (defined in code, no .inputactions file needed) ──
    InputActionMap actionMap;
    InputAction aimAction;
    InputAction fireAction;
    InputAction reloadAction;
    InputAction weaponWheelAction;
    InputAction flashlightAction;
    InputAction scopeToggleAction;

    // ── Public per-frame state ──────────────────────────────────────

    /// <summary>Right mouse button held OR left trigger held.</summary>
    public static bool Aim { get; private set; }
    /// <summary>Aim just pressed this frame.</summary>
    public static bool AimDown { get; private set; }
    /// <summary>Aim just released this frame.</summary>
    public static bool AimUp { get; private set; }

    /// <summary>Left mouse button held OR right trigger held.</summary>
    public static bool Fire { get; private set; }
    /// <summary>Fire just pressed this frame.</summary>
    public static bool FireDown { get; private set; }
    /// <summary>Fire just released this frame.</summary>
    public static bool FireUp { get; private set; }

    /// <summary>Reload pressed this frame (R / X).</summary>
    public static bool ReloadDown { get; private set; }

    /// <summary>Weapon wheel pressed (Tab / Select).</summary>
    public static bool WeaponWheelDown { get; private set; }
    /// <summary>Weapon wheel released (Tab / Select).</summary>
    public static bool WeaponWheelUp { get; private set; }

    /// <summary>Flashlight toggle pressed (T / RB).</summary>
    public static bool FlashlightDown { get; private set; }

    /// <summary>Sniper scope toggle pressed (E / R3).</summary>
    public static bool ScopeToggleDown { get; private set; }

    // ── Auto-bootstrap: creates itself before any scene loads ──
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

        // ── Aim: RMB / LT ──
        aimAction = actionMap.AddAction("Aim", type: InputActionType.Button);
        aimAction.AddBinding("<Mouse>/rightButton");
        aimAction.AddBinding("<Gamepad>/leftTrigger");

        // ── Fire: LMB / RT ──
        fireAction = actionMap.AddAction("Fire", type: InputActionType.Button);
        fireAction.AddBinding("<Mouse>/leftButton");
        fireAction.AddBinding("<Gamepad>/rightTrigger");

        // ── Reload: R / X (West face button) ──
        reloadAction = actionMap.AddAction("Reload", type: InputActionType.Button);
        reloadAction.AddBinding("<Keyboard>/r");
        reloadAction.AddBinding("<Gamepad>/buttonWest");

        // ── Weapon Wheel: Tab / Select (Back/View) ──
        weaponWheelAction = actionMap.AddAction("WeaponWheel", type: InputActionType.Button);
        weaponWheelAction.AddBinding("<Keyboard>/tab");
        weaponWheelAction.AddBinding("<Gamepad>/select");

        // ── Flashlight: T / RB ──
        flashlightAction = actionMap.AddAction("Flashlight", type: InputActionType.Button);
        flashlightAction.AddBinding("<Keyboard>/t");
        flashlightAction.AddBinding("<Gamepad>/rightShoulder");

        // ── Scope Toggle: E / R3 (Right Stick Press) ──
        scopeToggleAction = actionMap.AddAction("ScopeToggle", type: InputActionType.Button);
        scopeToggleAction.AddBinding("<Keyboard>/e");
        scopeToggleAction.AddBinding("<Gamepad>/rightStickPress");
    }

    void Update()
    {
        // ── Aim ──
        Aim     = aimAction.IsPressed();
        AimDown = aimAction.WasPressedThisFrame();
        AimUp   = aimAction.WasReleasedThisFrame();

        // ── Fire ──
        Fire     = fireAction.IsPressed();
        FireDown = fireAction.WasPressedThisFrame();
        FireUp   = fireAction.WasReleasedThisFrame();

        // ── Buttons ──
        ReloadDown      = reloadAction.WasPressedThisFrame();
        WeaponWheelDown = weaponWheelAction.WasPressedThisFrame();
        WeaponWheelUp   = weaponWheelAction.WasReleasedThisFrame();
        FlashlightDown  = flashlightAction.WasPressedThisFrame();
        ScopeToggleDown = scopeToggleAction.WasPressedThisFrame();
    }

    void OnEnable()  => actionMap?.Enable();
    void OnDisable() => actionMap?.Disable();

    void OnDestroy()
    {
        actionMap?.Disable();
        actionMap?.Dispose();
        if (Instance == this) Instance = null;
    }
}
