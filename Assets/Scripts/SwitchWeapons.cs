using System;
using UnityEngine;

public class SwitchWeapons : MonoBehaviour
{
    public static event Action<int> OnWeaponSwitch;

    [SerializeField] private GameObject pistol;
    [SerializeField] private GameObject shotgun;

    private int currentWeaponIndex = 1;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && currentWeaponIndex != 1)
        {
            OnWeaponSwitch?.Invoke(1);
            currentWeaponIndex = 1;
            shotgun.SetActive(false);
            pistol.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && currentWeaponIndex != 2)
        {
            OnWeaponSwitch?.Invoke(2);
            currentWeaponIndex = 2;
            pistol.SetActive(false);
            shotgun.SetActive(true);
        }
    }
}