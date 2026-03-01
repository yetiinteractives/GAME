using UnityEngine;

public class WeaponRadialBridge : MonoBehaviour
{
    [SerializeField] private RadialMenuController radialMenu;

    private void OnEnable()
    {
        if (radialMenu != null)
            radialMenu.OnItemSelected += HandleRadialSelection;
    }

    private void OnDisable()
    {
        if (radialMenu != null)
            radialMenu.OnItemSelected -= HandleRadialSelection;
    }

    private void HandleRadialSelection(RadialMenuController.RadialItem item, int index)
    {
        // Replace with your existing weapon switch call:
        // weaponManager.SwitchTo(item.id) OR weaponManager.SwitchToIndex(index)
        Debug.Log($"Bridge received selection: {item.displayName} / {item.id} / {index}");
    }
}