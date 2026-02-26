using UnityEngine;
using TMPro;

public class WeaponsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pistolIcon;
    [SerializeField] private GameObject shotgunIcon;
    [SerializeField] private GameObject sniperIcon;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text weaponStatusText;

    [Header("Weapon References")]
    [SerializeField] private Weapon pistolWeapon;
    [SerializeField] private Weapon shotgunWeapon;
    [SerializeField] private Weapon sniperWeapon;

    private Weapon currentWeapon;

    private void Start()
    {
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
        UnsubscribeFromWeapon();

        pistolIcon.SetActive(false);
        shotgunIcon.SetActive(false);
        sniperIcon.SetActive(false);

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

    private void UpdateAmmoDisplay(int bulletOnMag, int totalBullet)
    {
        if (ammoText != null)
            ammoText.text = $"{bulletOnMag} / {totalBullet}";
    }

    private void UpdateWeaponStatus(string status)
    {
        if (weaponStatusText != null)
        {
            weaponStatusText.text = status;

            if (status == "Ready" || status == "Aiming") weaponStatusText.color = Color.green;
            else if (status == "Reloading...") weaponStatusText.color = Color.yellow;
            else if (status == "Out of Ammo!") weaponStatusText.color = Color.red;
            else weaponStatusText.color = Color.white;
        }
    }
}