using System;
using UnityEngine;
using UnityEngine.UI;

public class CrossHairVisiliblity : MonoBehaviour
{
    public Image _crossHair;

    bool isWeaponSwtichUIActive = false;

    private void Start()
    {
        SwitchWeapons.OnToggleWeaponSwitchUI += HandleWeaponSwitchUI;
    }

    private void HandleWeaponSwitchUI(bool isWSUIon)
    {
        isWeaponSwtichUIActive = isWSUIon;
    }

    void Update()
    {
        //to show crosshair when right mouse button is held
        _crossHair.gameObject.SetActive(Input.GetMouseButton(1));



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
