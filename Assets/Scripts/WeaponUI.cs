using UnityEngine;
using TMPro;
using System;

public class WeaponsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pistolIcon;
    [SerializeField] private GameObject shotgunIcon;
    [SerializeField] private GameObject sniperIcon;
    [SerializeField] private GameObject grenadeIcon;
    [SerializeField] private GameObject landmineIcon;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text explosiveText;
    

    [Header("Weapon References")]
    [SerializeField] private Weapon pistolWeapon;
    [SerializeField] private Weapon shotgunWeapon;
    [SerializeField] private Weapon sniperWeapon;
    [SerializeField] private ExplosivesHandler explosivesHandler;

   


    private Weapon currentWeapon;

    private void Start()
    {
        SwitchToWeapon(1);
    }

    private void OnEnable()
    {
        SwitchWeapons.OnWeaponSwitch += OnWeaponSwitch;
        explosivesHandler.OnExplosivesCountChanged += OnExplosiveCountChanged;


    }

  

    private void OnDisable()
    {
        SwitchWeapons.OnWeaponSwitch -= OnWeaponSwitch;
        explosivesHandler.OnExplosivesCountChanged -= OnExplosiveCountChanged;
        UnsubscribeFromWeapon();
    }

    private void OnWeaponSwitch(int weaponIndex)
    {
        ammoText.gameObject.SetActive(weaponIndex<=3);
        explosiveText.gameObject.SetActive(weaponIndex>=4);

        SwitchToWeapon(weaponIndex);
    }

    private void SwitchToWeapon(int weaponIndex)
    {
        UnsubscribeFromWeapon();

        pistolIcon.SetActive(false);
        shotgunIcon.SetActive(false);
        sniperIcon.SetActive(false);
        grenadeIcon.SetActive(false);
        landmineIcon.SetActive(false);

        switch (weaponIndex)
        {
            case 1:
                currentWeapon = pistolWeapon;
                pistolIcon.SetActive(true);
                break;
            case 2:
                currentWeapon = shotgunWeapon;
                shotgunIcon.SetActive(true);
                break;
            case 3:
                currentWeapon = sniperWeapon;
                sniperIcon.SetActive(true);
                break;
            case 4:
                currentWeapon = null; 
                grenadeIcon.SetActive(true);
                break;
            case 5:
                currentWeapon = null;
                landmineIcon.SetActive(true);
                break;

            default:
                Debug.LogWarning($"Unknown weapon index: {weaponIndex}");
                return;
        }

        SubscribeToWeapon();
    }

    private void SubscribeToWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged += UpdateAmmoDisplay;
            
        }
    }

    private void UnsubscribeFromWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged -= UpdateAmmoDisplay;
            
        }
    }

    private void UpdateAmmoDisplay(int bulletOnMag, int totalBullet)
    {
        if (ammoText != null)
        {
            ammoText.text = $"{bulletOnMag} / {totalBullet}";
        }
    }

    private void OnExplosiveCountChanged(int count)
    {
        if(explosiveText != null)
        {
            explosiveText.text = $"{count}";
        }
    }

}