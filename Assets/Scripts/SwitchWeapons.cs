using System;
using TMPro;
using UnityEngine;

public class SwitchWeapons : MonoBehaviour
{
    public static event Action<int> OnWeaponSwitch;
    public static event Action<bool> OnToggleWeaponSwitchUI;

    [SerializeField] private GameObject pistol;
    [SerializeField] private GameObject shotgun;
    [SerializeField] private GameObject sniper;
    [SerializeField] private ExplosivesHandler explosivesHandler;
    [SerializeField] private GameObject switchWeaponWheel;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Healing UI")]
    [SerializeField] private TextMeshProUGUI bandageTMP;
    [SerializeField] private TextMeshProUGUI medikitTMP;

    private int currentWeaponIndex;
    private bool isWheelOpen;

    private int bandageCount;
    private int medikitCount;

    private void Start()
    {
        if (switchWeaponWheel != null) switchWeaponWheel.SetActive(false);
        SyncFromResourceManager();
        OnPistolSelected();
    }

    private void OnEnable()
    {
        SyncFromResourceManager();
    }

    private void Update()
    {
        if (GameInput.WeaponWheelDown) OpenWheel();
        if (isWheelOpen && GameInput.WeaponWheelUp) CloseWheel();
    }

    private void OpenWheel()
    {
        if (isWheelOpen) return;
        isWheelOpen = true;
        Time.timeScale = 0f;
        if (switchWeaponWheel != null) switchWeaponWheel.SetActive(true);
        OnToggleWeaponSwitchUI?.Invoke(true);
    }

    private void CloseWheel()
    {
        if (!isWheelOpen) return;
        isWheelOpen = false;
        Time.timeScale = 1f;
        if (switchWeaponWheel != null) switchWeaponWheel.SetActive(false);
        OnToggleWeaponSwitchUI?.Invoke(false);
    }

    private void DisableWeapons()
    {
        if (pistol != null) pistol.SetActive(false);
        if (shotgun != null) shotgun.SetActive(false);
        if (sniper != null) sniper.SetActive(false);
    }

    public void OnPistolSelected()
    {
        if (currentWeaponIndex == 1) return;
        DisableWeapons();
        currentWeaponIndex = 1;
        if (pistol != null) pistol.SetActive(true);
        OnWeaponSwitch?.Invoke(1);
        CloseWheel();
        RefreshCurrentWeaponUI();
    }

    public void OnShotgunSelected()
    {
        if (currentWeaponIndex == 2) return;
        DisableWeapons();
        currentWeaponIndex = 2;
        if (shotgun != null) shotgun.SetActive(true);
        OnWeaponSwitch?.Invoke(2);
        CloseWheel();
        RefreshCurrentWeaponUI();
    }

    public void OnSniperSelected()
    {
        if (currentWeaponIndex == 3) return;
        DisableWeapons();
        currentWeaponIndex = 3;
        if (sniper != null) sniper.SetActive(true);
        OnWeaponSwitch?.Invoke(3);
        CloseWheel();
        RefreshCurrentWeaponUI();
    }

    public void OnGrenadeSelected()
    {
        if (currentWeaponIndex == 4) return;
        DisableWeapons();
        currentWeaponIndex = 4;
        OnWeaponSwitch?.Invoke(4);
        CloseWheel();
        RefreshCurrentWeaponUI();
    }

    public void OnLandmineSelected()
    {
        if (currentWeaponIndex == 5) return;
        DisableWeapons();
        currentWeaponIndex = 5;
        OnWeaponSwitch?.Invoke(5);
        CloseWheel();
        RefreshCurrentWeaponUI();
    }

    public void OnBandageSelected()
    {
        if (bandageCount <= 0 || playerHealth == null || ResourceManager.Instance == null) return;

        playerHealth.HealPlayer(25f);

        // SET architecture: compute then set absolute value
        int next = Mathf.Max(0, ResourceManager.Instance.BandageCount - 1);
        ResourceManager.Instance.SetBandageAbsolute(next);

        SyncFromResourceManager();
        InventoryHandler.Instance?.SyncFromResourceManagerForUI();
    }

    public void OnMedikitSelected()
    {
        if (medikitCount <= 0 || playerHealth == null || ResourceManager.Instance == null) return;

        playerHealth.HealPlayer(100f);

        // SET architecture: compute then set absolute value
        int next = Mathf.Max(0, ResourceManager.Instance.MedkitCount - 1);
        ResourceManager.Instance.SetMedkitAbsolute(next);

        SyncFromResourceManager();
        InventoryHandler.Instance?.SyncFromResourceManagerForUI();
    }

    public void SyncFromResourceManager()
    {
        if (ResourceManager.Instance == null) return;

        bandageCount = ResourceManager.Instance.BandageCount;
        medikitCount = ResourceManager.Instance.MedkitCount;

        if (bandageTMP != null) bandageTMP.text = bandageCount.ToString();
        if (medikitTMP != null) medikitTMP.text = medikitCount.ToString();
    }
    private void RefreshCurrentWeaponUI()
    {
        Weapon w = null;

        if (currentWeaponIndex == 1 && pistol != null) w = pistol.GetComponent<Weapon>();
        else if (currentWeaponIndex == 2 && shotgun != null) w = shotgun.GetComponent<Weapon>();
        else if (currentWeaponIndex == 3 && sniper != null) w = sniper.GetComponent<Weapon>();

        w?.ForceAmmoUIRefresh();

        // explosives UI refresh too
        if (currentWeaponIndex == 4)
            explosivesHandler?.SetCounts(ResourceManager.Instance.GrenadeCount, ResourceManager.Instance.LandmineCount);
        else if (currentWeaponIndex == 5)
            explosivesHandler?.SetCounts(ResourceManager.Instance.GrenadeCount, ResourceManager.Instance.LandmineCount);
    }
}