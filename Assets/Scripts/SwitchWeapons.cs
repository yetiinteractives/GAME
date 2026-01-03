using System;
using UnityEngine;

public class SwitchWeapons : MonoBehaviour
{
    public static event Action<int> OnWeaponSwitch;

    [SerializeField] private GameObject pistol;
    [SerializeField] private GameObject shotgun;
    [SerializeField] private GameObject sniper;

    private int currentWeaponIndex = 1;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && currentWeaponIndex != 1)
        {
            OnWeaponSwitch?.Invoke(1);
            DisableWeapons();
            currentWeaponIndex = 1;
            pistol.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && currentWeaponIndex != 2)
        {
            OnWeaponSwitch?.Invoke(2);
            DisableWeapons();
            currentWeaponIndex = 2;
            shotgun.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && currentWeaponIndex != 3)
        {
            OnWeaponSwitch?.Invoke(3);
            DisableWeapons();
            currentWeaponIndex = 3;
            sniper.SetActive(true);
        }
    }

    private void DisableWeapons()
    {
        pistol.SetActive(false);
        shotgun.SetActive(false);   
        sniper.SetActive(false);
    }
}