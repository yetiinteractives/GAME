using UnityEngine;
using TMPro;

public class WeaponsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pistolIcon;
    [SerializeField] private GameObject shotgunIcon;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text weaponStatusText;

    [Header("Weapon References")]
    [SerializeField] private Weapon pistolWeapon;
    [SerializeField] private Weapon shotgunWeapon;

    private Weapon currentWeapon;

    private void Start()
    {
        // Start with pistol
        SwitchToWeapon(1);
    }

    private void OnEnable()
    {
        SwitchWeapons.OnWeaponSwitch += OnWeaponSwitch;
    }

    private void OnDisable()
    {
        SwitchWeapons.OnWeaponSwitch -= OnWeaponSwitch;
        UnsubscribeFromWeapon();
    }

    private void OnWeaponSwitch(int weaponIndex)
    {
        SwitchToWeapon(weaponIndex);
    }

    private void SwitchToWeapon(int weaponIndex)
    {
        // Unsubscribe from old weapon
        UnsubscribeFromWeapon();

        // Switch weapon
        if (weaponIndex == 1)
        {
            currentWeapon = pistolWeapon;
            pistolIcon.SetActive(true);
            shotgunIcon.SetActive(false);
        }
        else if (weaponIndex == 2)
        {
            currentWeapon = shotgunWeapon;
            pistolIcon.SetActive(false);
            shotgunIcon.SetActive(true);
        }

        // Subscribe to new weapon
        SubscribeToWeapon();

        // Update UI immediately
        if (currentWeapon != null)
        {
            UpdateAmmoDisplay(currentWeapon.CurrentAmmo);
            UpdateWeaponStatus(currentWeapon.IsReloading ? "Reloading..." :
                              currentWeapon.CurrentAmmo == 0 ? "Out of Ammo!" : "Ready");
        }
    }

    private void SubscribeToWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged += UpdateAmmoDisplay;
            currentWeapon.OnWeaponStatusChanged += UpdateWeaponStatus;
        }
    }

    private void UnsubscribeFromWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged -= UpdateAmmoDisplay;
            currentWeapon.OnWeaponStatusChanged -= UpdateWeaponStatus;
        }
    }

    private void UpdateAmmoDisplay(int currentAmmo)
    {
        if (ammoText != null && currentWeapon != null)
        {
            ammoText.text = $"{currentAmmo}/{currentWeapon.MagazineSize}";
        }
    }

    private void UpdateWeaponStatus(string status)
    {
        if (weaponStatusText != null)
        {
            weaponStatusText.text = status;

            // Color code
            if (status == "Ready") weaponStatusText.color = Color.green;
            else if (status == "Reloading...") weaponStatusText.color = Color.yellow;
            else if (status == "Out of Ammo!") weaponStatusText.color = Color.red;
            else weaponStatusText.color = Color.white;
        }
    }
}