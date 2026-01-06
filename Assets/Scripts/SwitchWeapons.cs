using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SwitchWeapons : MonoBehaviour
{
    public static event Action<int> OnWeaponSwitch;  // Event to switch weapons , to pass the weapon index
    public static event Action<bool> OnToggleWeaponSwitchUI;  // Event to toggle weapon switch UI , using to pause the game

    // References to weapon GameObjects
    [SerializeField] private GameObject pistol;
    [SerializeField] private GameObject shotgun;
    [SerializeField] private GameObject sniper;

    // Reference to the weapon switch UI
    [SerializeField] private GameObject switchWeaponWheel;

    private int currentWeaponIndex = 1;
    private bool isWheelOpen = false;

    private void Start()
    {
        if(switchWeaponWheel != null)
        {
            switchWeaponWheel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OpenWheel();
        }

        // Only close if wheel is open AND Tab is released
        if (isWheelOpen && Input.GetKeyUp(KeyCode.Tab))
        {
            CloseWheel();
        }


    }
    private void OpenWheel()
    {
        if (isWheelOpen) return;

        isWheelOpen = true;
        Time.timeScale = 0f;
        switchWeaponWheel.SetActive(true);
        OnToggleWeaponSwitchUI?.Invoke(isWheelOpen);
    }

    private void CloseWheel()
    {
        if (!isWheelOpen) return;

        isWheelOpen = false;
        Time.timeScale = 1f;
        switchWeaponWheel.SetActive(false);
        OnToggleWeaponSwitchUI?.Invoke(isWheelOpen);
    }


    private void DisableWeapons() //disabling all weapons before enabling the selected one
    {
        pistol.SetActive(false);
        shotgun.SetActive(false);   
        sniper.SetActive(false);
    }

    public void OnPistolSelected()
    {
        if (currentWeaponIndex != 1) 
        {
            OnWeaponSwitch?.Invoke(1);
            DisableWeapons();
            currentWeaponIndex = 1;
            pistol.SetActive(true);

            CloseWheel();

            
            
        }

    }
    public void OnShotgunSelected()
    {
        if (currentWeaponIndex != 2) 
        {
            OnWeaponSwitch?.Invoke(2);
            DisableWeapons();
            currentWeaponIndex = 2;
            shotgun.SetActive(true);

            CloseWheel();

            
        }
    }
    public void OnSniperSelected()
    {
        if (currentWeaponIndex != 3) 
        {
            OnWeaponSwitch?.Invoke(3);
            DisableWeapons();
            currentWeaponIndex = 3;
            sniper.SetActive(true);

            CloseWheel();

           
            
        }
    }
    public void OnHealingSelected()
    {
        CloseWheel();

  
        
    }
}