using System;
using UnityEngine;
using UnityEngine.UI;

public class CrossHairVisiliblity : MonoBehaviour
{
    public Image _crossHair;
    public Image _shotgunCrossHair;

    bool isWeaponSwtichUIActive = false;
    bool isAiming = false;
    int currentWeaponIndex = 1;

    private void Start()
    {
        SwitchWeapons.OnToggleWeaponSwitchUI += HandleWeaponSwitchUI;
        SwitchWeapons.OnWeaponSwitch += HandleWeaponSwitch;
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
        
        if (Input.GetMouseButton(1))
        {
            isAiming = true;
        }
        else
        {
            isAiming = false;
        }

        // Crosshair visibility logic
        _shotgunCrossHair.gameObject.SetActive(currentWeaponIndex == 2 && isAiming); 
        _crossHair.gameObject.SetActive(((currentWeaponIndex == 1) || (currentWeaponIndex == 3))  && isAiming);




        if (!isWeaponSwtichUIActive)
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
}
