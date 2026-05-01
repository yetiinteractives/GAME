using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Pistol pistol;
    [SerializeField] private Shotgun shotgun;
    [SerializeField] private Sniper sniper;
    [SerializeField] private ExplosivesHandler explosives;
    [SerializeField] private SwitchWeapons switchWeapons;
    [SerializeField] private InventoryHandler inventoryHandler;

    public static Action<bool> OnPistolAmmoFullChanged;
    public static Action<bool> OnShotgunAmmoFullChanged;
    public static Action<bool> OnSniperAmmoFullChanged;
    public static Action<bool> OnGrenadeFullChanged;
    public static Action<bool> OnLandmineFullChanged;
    public static Action<bool> OnMedkitFullChanged;
    public static Action<bool> OnBandageFullChanged;
    public static Action<bool> OnShotgunShellFullChanged;
    public static Action<bool> OnSilencerFullChanged;
    public static Action<bool> OnAlcoholFullChanged;
    public static Action<bool> OnRagFullChanged;
    public static Action<bool> OnBindingFullChanged;
    public static Action<bool> OnGunpowderFullChanged;
    public static Action<bool> OnCanFullChanged;

    [Header("Ammo (Reserve)")]
    [SerializeField] private int pistolAmmoCount = 40;
    [SerializeField] private int pistolAmmoMax = 90;
    [SerializeField] private int shotgunAmmoCount = 10;
    [SerializeField] private int shotgunAmmoMax = 30;
    [SerializeField] private int sniperAmmoCount = 10;
    [SerializeField] private int sniperAmmoMax = 30;

    [Header("Ammo (Magazine Runtime)")]
    [SerializeField] private int pistolMagAmmo = 0;
    [SerializeField] private int shotgunMagAmmo = 0;
    [SerializeField] private int sniperMagAmmo = 0;

    [Header("Explosives")]
    [SerializeField] private int grenadeCount = 2;
    [SerializeField] private int grenadeMax = 5;
    [SerializeField] private int landmineCount = 2;
    [SerializeField] private int landmineMax = 5;

    [Header("Craftables")]
    [SerializeField] private int medkitCount = 0;
    [SerializeField] private int medkitMax = 2;
    [SerializeField] private int bandageCount = 0;
    [SerializeField] private int bandageMax = 6;
    [SerializeField] private int shotgunShellCount = 0;
    [SerializeField] private int shotgunShellMax = 30;
    [SerializeField] private int silencerCount = 0;
    [SerializeField] private int silencerMax = 3;

    [Header("Ingredients")]
    [SerializeField] private int alcoholCount = 3;
    [SerializeField] private int alcoholMax = 5;
    [SerializeField] private int ragCount = 3;
    [SerializeField] private int ragMax = 5;
    [SerializeField] private int bindingCount = 5;
    [SerializeField] private int bindingMax = 10;
    [SerializeField] private int gunpowderCount = 5;
    [SerializeField] private int gunpowderMax = 10;
    [SerializeField] private int canCount = 3;
    [SerializeField] private int canMax = 5;

    [Header("Pistol Silencer Runtime State")]
    [SerializeField] private bool isPistolSilencerEquipped = false;
    [SerializeField] private int pistolSilencerDurability = 10;

    // Getters
    public int PistolAmmoCount => pistolAmmoCount;
    public int ShotgunAmmoCount => shotgunAmmoCount;
    public int SniperAmmoCount => sniperAmmoCount;

    public int PistolMagAmmo => pistolMagAmmo;
    public int ShotgunMagAmmo => shotgunMagAmmo;
    public int SniperMagAmmo => sniperMagAmmo;

    public int GrenadeCount => grenadeCount;
    public int LandmineCount => landmineCount;

    public int MedkitCount => medkitCount;
    public int BandageCount => bandageCount;
    public int ShotgunShellCount => shotgunShellCount;
    public int SilencerCount => silencerCount;

    public int AlcoholCount => alcoholCount;
    public int RagCount => ragCount;
    public int BindingCount => bindingCount;
    public int GunpowderCount => gunpowderCount;
    public int CanCount => canCount;



    public bool IsPistolSilencerEquipped => isPistolSilencerEquipped;
    public int PistolSilencerDurability => pistolSilencerDurability;

    public void BroadcastAllFullStatesPublic() => BroadcastAllFullStates();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ClampAll();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        ForceResyncAllRuntimeUsers();
        BroadcastAllFullStates();
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ForceResyncAllRuntimeUsers();
        BroadcastAllFullStates();
    }

    private void RebindRefs()
    {
        pistol = FindFirstObjectByType<Pistol>(FindObjectsInactive.Include);
        shotgun = FindFirstObjectByType<Shotgun>(FindObjectsInactive.Include);
        sniper = FindFirstObjectByType<Sniper>(FindObjectsInactive.Include);
        explosives = FindFirstObjectByType<ExplosivesHandler>(FindObjectsInactive.Include);
        switchWeapons = FindFirstObjectByType<SwitchWeapons>(FindObjectsInactive.Include);
        inventoryHandler = FindFirstObjectByType<InventoryHandler>(FindObjectsInactive.Include);
    }

    public void ForceResyncAllRuntimeUsers()
    {
        RebindRefs();

        if (pistol != null) pistol.SetAmmoState(pistolMagAmmo, pistolAmmoCount);
        if (shotgun != null) shotgun.SetAmmoState(shotgunMagAmmo, shotgunAmmoCount);
        if (sniper != null) sniper.SetAmmoState(sniperMagAmmo, sniperAmmoCount);

        explosives?.SetCounts(grenadeCount, landmineCount);

        switchWeapons?.SyncFromResourceManager();
        inventoryHandler?.SyncFromResourceManagerForUI();

        pistol?.ForceAmmoUIRefresh();
        shotgun?.ForceAmmoUIRefresh();
        sniper?.ForceAmmoUIRefresh();
        explosives?.SetCounts(grenadeCount, landmineCount);
    }

    public void SetPistolMagAmmo(int value) => pistolMagAmmo = Mathf.Max(0, value);
    public void SetShotgunMagAmmo(int value) => shotgunMagAmmo = Mathf.Max(0, value);
    public void SetSniperMagAmmo(int value) => sniperMagAmmo = Mathf.Max(0, value);

    public void SetShotgunReserveAbsolute(int value)
    {
        shotgunAmmoCount = Mathf.Clamp(value, 0, shotgunAmmoMax);
        shotgunShellCount = Mathf.Clamp(shotgunAmmoCount, 0, shotgunShellMax);
    }

    public void SetPistolSilencerRuntimeState(bool equipped, int durability)
    {
        isPistolSilencerEquipped = equipped;
        pistolSilencerDurability = Mathf.Clamp(durability, 0, 10);
    }

    public void CaptureRuntimeAmmoFromWeapons()
    {
        RebindRefs();

        if (pistol != null) { pistolMagAmmo = pistol.GetMagAmmo(); pistolAmmoCount = pistol.GetReserveAmmo(); }
        if (shotgun != null)
        {
            shotgunMagAmmo = shotgun.GetMagAmmo();
            shotgunAmmoCount = shotgun.GetReserveAmmo();
            shotgunShellCount = Mathf.Clamp(shotgunAmmoCount, 0, shotgunShellMax);
        }
        if (sniper != null) { sniperMagAmmo = sniper.GetMagAmmo(); sniperAmmoCount = sniper.GetReserveAmmo(); }

        ClampAll();
    }

    public void AddPistolReserve(int amount)
    {
        CaptureRuntimeAmmoFromWeapons(); // keeps current mag
        SetPistolReserveAbsolute(pistolAmmoCount + amount);
        ForceResyncAllRuntimeUsers();
        BroadcastAllFullStates();
    }

    public void AddShotgunReserve(int amount)
    {
        CaptureRuntimeAmmoFromWeapons();
        SetShotgunReserveAbsolute(shotgunAmmoCount + amount);
        ForceResyncAllRuntimeUsers();
        BroadcastAllFullStates();
    }

    public void AddSniperReserve(int amount)
    {
        CaptureRuntimeAmmoFromWeapons();
        SetSniperReserveAbsolute(sniperAmmoCount + amount);
        ForceResyncAllRuntimeUsers();
        BroadcastAllFullStates();
    }

    public ResourceSaveData ExportSaveData()
    {
        return new ResourceSaveData
        {
            pistolAmmoCount = pistolAmmoCount,
            shotgunAmmoCount = shotgunAmmoCount,
            sniperAmmoCount = sniperAmmoCount,
            pistolMagAmmo = pistolMagAmmo,
            shotgunMagAmmo = shotgunMagAmmo,
            sniperMagAmmo = sniperMagAmmo,
            grenadeCount = grenadeCount,
            landmineCount = landmineCount,
            medkitCount = medkitCount,
            bandageCount = bandageCount,
            shotgunShellCount = shotgunShellCount,
            silencerCount = silencerCount,
            alcoholCount = alcoholCount,
            ragCount = ragCount,
            bindingCount = bindingCount,
            gunpowderCount = gunpowderCount,
            canCount = canCount,
            isPistolSilencerEquipped = isPistolSilencerEquipped,
            pistolSilencerDurability = pistolSilencerDurability
        };
    }

    public void ImportSaveData(ResourceSaveData d)
    {
        if (d == null) return;

        pistolAmmoCount = d.pistolAmmoCount;
        shotgunAmmoCount = d.shotgunAmmoCount;
        sniperAmmoCount = d.sniperAmmoCount;

        pistolMagAmmo = d.pistolMagAmmo;
        shotgunMagAmmo = d.shotgunMagAmmo;
        sniperMagAmmo = d.sniperMagAmmo;

        grenadeCount = d.grenadeCount;
        landmineCount = d.landmineCount;

        medkitCount = d.medkitCount;
        bandageCount = d.bandageCount;
        shotgunShellCount = d.shotgunShellCount;
        silencerCount = d.silencerCount;

        alcoholCount = d.alcoholCount;
        ragCount = d.ragCount;
        bindingCount = d.bindingCount;
        gunpowderCount = d.gunpowderCount;
        canCount = d.canCount;

        isPistolSilencerEquipped = d.isPistolSilencerEquipped;
        pistolSilencerDurability = d.pistolSilencerDurability;

        ClampAll();
        ForceResyncAllRuntimeUsers();
        BroadcastAllFullStates();
    }

    public void ResetToDefaults()
    {
        pistolAmmoCount = 40; shotgunAmmoCount = 10; sniperAmmoCount = 10;
        pistolMagAmmo = 0; shotgunMagAmmo = 0; sniperMagAmmo = 0;
        grenadeCount = 2; landmineCount = 2;
        medkitCount = 0; bandageCount = 0; shotgunShellCount = 0; silencerCount = 0;
        alcoholCount = 3; ragCount = 3; bindingCount = 5; gunpowderCount = 5; canCount = 3;
        isPistolSilencerEquipped = false; pistolSilencerDurability = 10;

        ClampAll();
        ForceResyncAllRuntimeUsers();
        BroadcastAllFullStates();
    }

    private void ClampAll()
    {
        pistolAmmoCount = Mathf.Clamp(pistolAmmoCount, 0, pistolAmmoMax);
        shotgunAmmoCount = Mathf.Clamp(shotgunAmmoCount, 0, shotgunAmmoMax);
        sniperAmmoCount = Mathf.Clamp(sniperAmmoCount, 0, sniperAmmoMax);

        grenadeCount = Mathf.Clamp(grenadeCount, 0, grenadeMax);
        landmineCount = Mathf.Clamp(landmineCount, 0, landmineMax);

        medkitCount = Mathf.Clamp(medkitCount, 0, medkitMax);
        bandageCount = Mathf.Clamp(bandageCount, 0, bandageMax);
        shotgunShellCount = Mathf.Clamp(shotgunShellCount, 0, shotgunShellMax);
        silencerCount = Mathf.Clamp(silencerCount, 0, silencerMax);

        alcoholCount = Mathf.Clamp(alcoholCount, 0, alcoholMax);
        ragCount = Mathf.Clamp(ragCount, 0, ragMax);
        bindingCount = Mathf.Clamp(bindingCount, 0, bindingMax);
        gunpowderCount = Mathf.Clamp(gunpowderCount, 0, gunpowderMax);
        canCount = Mathf.Clamp(canCount, 0, canMax);

        shotgunShellCount = Mathf.Clamp(shotgunAmmoCount, 0, shotgunShellMax);

        pistolSilencerDurability = Mathf.Clamp(pistolSilencerDurability, 0, 10);
        if (silencerCount <= 0) isPistolSilencerEquipped = false;
    }

    private void BroadcastAllFullStates()
    {
        OnPistolAmmoFullChanged?.Invoke(false);
        OnShotgunAmmoFullChanged?.Invoke(false);
        OnSniperAmmoFullChanged?.Invoke(false);
        OnGrenadeFullChanged?.Invoke(false);
        OnLandmineFullChanged?.Invoke(false);
        OnMedkitFullChanged?.Invoke(false);
        OnBandageFullChanged?.Invoke(false);
        OnShotgunShellFullChanged?.Invoke(false);
        OnSilencerFullChanged?.Invoke(false);
        OnAlcoholFullChanged?.Invoke(false);
        OnRagFullChanged?.Invoke(false);
        OnBindingFullChanged?.Invoke(false);
        OnGunpowderFullChanged?.Invoke(false);
        OnCanFullChanged?.Invoke(false);
    }

    public void SetBandageAbsolute(int value) => bandageCount = Mathf.Clamp(value, 0, bandageMax);
    public void SetMedkitAbsolute(int value) => medkitCount = Mathf.Clamp(value, 0, medkitMax);
    public void SetGrenadeAbsolute(int value) => grenadeCount = Mathf.Clamp(value, 0, grenadeMax);
    public void SetLandmineAbsolute(int value) => landmineCount = Mathf.Clamp(value, 0, landmineMax);
    public void SetShotgunShellAbsolute(int value)
    {
        shotgunShellCount = Mathf.Clamp(value, 0, shotgunShellMax);
        shotgunAmmoCount = Mathf.Clamp(shotgunShellCount, 0, shotgunAmmoMax);
    }
    public void SetSilencerAbsolute(int value) => silencerCount = Mathf.Clamp(value, 0, silencerMax);

    public void SetAlcoholAbsolute(int value) => alcoholCount = Mathf.Clamp(value, 0, alcoholMax);
    public void SetRagAbsolute(int value) => ragCount = Mathf.Clamp(value, 0, ragMax);
    public void SetBindingAbsolute(int value) => bindingCount = Mathf.Clamp(value, 0, bindingMax);
    public void SetGunpowderAbsolute(int value) => gunpowderCount = Mathf.Clamp(value, 0, gunpowderMax);
    public void SetCanAbsolute(int value) => canCount = Mathf.Clamp(value, 0, canMax);



    public void SetPistolReserveAbsolute(int value) => pistolAmmoCount = Mathf.Clamp(value, 0, pistolAmmoMax);
    public void SetSniperReserveAbsolute(int value) => sniperAmmoCount = Mathf.Clamp(value, 0, sniperAmmoMax);

}