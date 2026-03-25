using System;
using UnityEngine;
using UnityEngine.UI;

public class CrossHairVisiliblity : MonoBehaviour
{
    [SerializeField] private Image _crossHair;
    [SerializeField] private Image _shotgunCrossHair;
    [SerializeField] private Image _sniperCrossHair;

    bool isWeaponSwtichUIActive = false;
    bool isAiming = false;
    bool isScoped = false;
    bool isPauseMenuOn = false;
    bool isInventoryOn = false;

    int currentWeaponIndex = 1;


    private void Start()
    {
        SwitchWeapons.OnToggleWeaponSwitchUI += HandleWeaponSwitchUI;
        SwitchWeapons.OnWeaponSwitch += HandleWeaponSwitch;
        MousePosition3D.OnFirePerformed += HandleFirePerformed;
        Sniper.OnSniperStatusUpdate += HandleSniperStatusUpdate;
        PauseMenuHandler.OnPauseMenuToggled += HandlePauseMenuToggled;
        InventoryHandler.OnInventoryToggled += HandleInventoryToggled;

    }

    private void HandleInventoryToggled(bool isOn)
    {
        isInventoryOn = isOn;
    }

    private void HandlePauseMenuToggled(bool isPauseOn)
    {
        isPauseMenuOn = isPauseOn;
    }

    private void HandleSniperStatusUpdate(bool isScopeOn)
    {
        isScoped = isScopeOn;
    }

    private void HandleFirePerformed(RaycastHit hit)
    {
        _shotgunCrossHair.gameObject.SetActive(false);
        _crossHair.gameObject.SetActive(false);
    }

    private void HandleWeaponSwitch(int weaponIndex)
    {
       currentWeaponIndex = weaponIndex;
    }

    private void HandleWeaponSwitchUI(bool isWSUIon)
    {
        isWeaponSwtichUIActive = isWSUIon;
    }

    void Update()
    {
        
        isAiming = GameInput.Aim;

        // Crosshair visibility logic
        _sniperCrossHair.gameObject.SetActive(((currentWeaponIndex == 3)) && isAiming && !isScoped);
        _shotgunCrossHair.gameObject.SetActive(currentWeaponIndex == 2 && isAiming); 
        _crossHair.gameObject.SetActive(((currentWeaponIndex == 1))  && isAiming);
        



        if (!isWeaponSwtichUIActive && !isPauseMenuOn &&!isInventoryOn)
        {
            //to hide cursor 
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;  
        }
        else
        {
            //to show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }


    }

    private void OnDisable()
    {
        
        SwitchWeapons.OnToggleWeaponSwitchUI -= HandleWeaponSwitchUI;
        SwitchWeapons.OnWeaponSwitch -= HandleWeaponSwitch;
        MousePosition3D.OnFirePerformed -= HandleFirePerformed;
        Sniper.OnSniperStatusUpdate -= HandleSniperStatusUpdate;
        PauseMenuHandler.OnPauseMenuToggled -= HandlePauseMenuToggled;
    }
}
