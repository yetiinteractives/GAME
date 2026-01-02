using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class SwitchWeapons : MonoBehaviour
{
    public static event Action<int> OnWeaponSwitch;
    
    [SerializeField] private Image pistolIcon;
    [SerializeField] private Image shotGunIcon;

    [SerializeField] private GameObject pistol;
    [SerializeField] private GameObject shotGun;



    private int currentWeaponIndex = 1;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && (currentWeaponIndex != 1))
        {
            
            //switch to pistol
            OnWeaponSwitch?.Invoke(1);
            currentWeaponIndex = 1;

            shotGunIcon.gameObject.SetActive(false);
            pistolIcon.gameObject.SetActive(true);

            shotGun.SetActive(false);
            pistol.SetActive(true);

           

        }
        else if ((Input.GetKeyDown(KeyCode.Alpha2)) && (currentWeaponIndex != 2))
        {
            //switch to ShotGun
            OnWeaponSwitch?.Invoke(2);
            currentWeaponIndex = 2;

            pistolIcon.gameObject.SetActive(false);
            shotGunIcon.gameObject.SetActive(true);

            pistol.SetActive(false);
            shotGun.SetActive(true);

        }

    }
}
