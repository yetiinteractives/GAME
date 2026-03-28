using System;
using System.Collections;
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

    // Full-state events
    public static Action<bool> OnPistolAmmoFullChanged;
    public static Action<bool> OnShotgunAmmoFullChanged;
    public static Action<bool> OnSniperAmmoFullChanged;
    public static Action<bool> OnGrenadeFullChanged;
    public static Action<bool> OnLandmineFullChanged;
    public static Action<bool> OnMedkitFullChanged;
    public static Action<bool> OnBandageFullChanged;
    public static Action<bool> OnSilencerFullChanged;
    public static Action<bool> OnAlcoholFullChanged;
    public static Action<bool> OnRagFullChanged;
    public static Action<bool> OnBindingFullChanged;
    public static Action<bool> OnGunpowderFullChanged;
    public static Action<bool> OnCanFullChanged;

    [Header("Ammo")]
    [SerializeField] private int pistolAmmoCount = 40;
    [SerializeField] private int pistolAmmoMax = 90;
    [SerializeField] private int shotgunAmmoCount = 10;
    [SerializeField] private int shotgunAmmoMax = 30;
    [SerializeField] private int sniperAmmoCount = 10;
    [SerializeField] private int sniperAmmoMax = 30;

    [Header("Explosives")]
    [SerializeField] private int grenadeCount = 2;
    [SerializeField] private int grenadeMax = 5;
    [SerializeField] private int landmineCount = 2;
    [SerializeField] private int landmineMax = 5;

    [Header("Craftables")]
    [SerializeField] private int medkitCount = 2;
    [SerializeField] private int medkitMax = 3;
    [SerializeField] private int bandageCount = 4;
    [SerializeField] private int bandageMax = 6;
    [SerializeField] private int silencerCount = 1;
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

    private Coroutine reloadSyncRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ClampAll();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        BroadcastAllFullStates();
        BeginReloadSync();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BeginReloadSync();
    }

    private void BeginReloadSync()
    {
        if (reloadSyncRoutine != null) StopCoroutine(reloadSyncRoutine);
        reloadSyncRoutine = StartCoroutine(ReloadSyncRoutine());
    }

    private IEnumerator ReloadSyncRoutine()
    {
        // wait a bit so scene scripts are alive and at their initial 0 values
        yield return null;
        yield return null;

        RebindSceneRefs();

        // push manager truth -> scene objects (they start at 0)
        pistol?.AddBullets(pistolAmmoCount);
        shotgun?.AddBullets(shotgunAmmoCount);
        sniper?.AddBullets(sniperAmmoCount);

        explosives?.AddGrenades(grenadeCount);
        explosives?.AddLandmines(landmineCount);

        switchWeapons?.AddBandage(bandageCount);

        inventoryHandler?.AddAlcohol(alcoholCount);
        inventoryHandler?.AddRag(ragCount);
        inventoryHandler?.AddBinding(bindingCount);
        inventoryHandler?.AddGunPowder(gunpowderCount);
        inventoryHandler?.AddCan(canCount);

        BroadcastAllFullStates();
        reloadSyncRoutine = null;
    }

    private void RebindSceneRefs()
    {
        pistol = FindFirstObjectByType<Pistol>(FindObjectsInactive.Include);
        shotgun = FindFirstObjectByType<Shotgun>(FindObjectsInactive.Include);
        sniper = FindFirstObjectByType<Sniper>(FindObjectsInactive.Include);
        explosives = FindFirstObjectByType<ExplosivesHandler>(FindObjectsInactive.Include);
        switchWeapons = FindFirstObjectByType<SwitchWeapons>(FindObjectsInactive.Include);
        inventoryHandler = FindFirstObjectByType<InventoryHandler>(FindObjectsInactive.Include);
    }

    // ---------- centralized clamp ----------
    private int AddClamped(ref int count, int max, int request, Action<bool> fullEvt)
    {
        if (request <= 0) return 0;
        int before = count;
        int accepted = Mathf.Clamp(request, 0, Mathf.Max(0, max - count));
        count += accepted;
        EmitFullChanged(before, count, max, fullEvt);
        return accepted;
    }

    private bool ConsumeClamped(ref int count, int max, int amount, Action<bool> fullEvt)
    {
        if (amount <= 0) return true;
        if (count < amount) return false;

        int before = count;
        count = Mathf.Clamp(count - amount, 0, max);
        EmitFullChanged(before, count, max, fullEvt);
        return true;
    }

    private void EmitFullChanged(int before, int after, int max, Action<bool> evt)
    {
        bool wasFull = before >= max;
        bool isFull = after >= max;
        if (wasFull != isFull) evt?.Invoke(isFull);
    }

    // ---------- add APIs ----------
    public int SetPistolAmmo(int amount)
    {
        int accepted = AddClamped(ref pistolAmmoCount, pistolAmmoMax, amount, OnPistolAmmoFullChanged);
        if (accepted > 0) pistol?.AddBullets(accepted);
        return accepted;
    }

    public int SetShotgunAmmo(int amount)
    {
        int accepted = AddClamped(ref shotgunAmmoCount, shotgunAmmoMax, amount, OnShotgunAmmoFullChanged);
        if (accepted > 0) shotgun?.AddBullets(accepted);
        return accepted;
    }

    public int SetSniperAmmo(int amount)
    {
        int accepted = AddClamped(ref sniperAmmoCount, sniperAmmoMax, amount, OnSniperAmmoFullChanged);
        if (accepted > 0) sniper?.AddBullets(accepted);
        return accepted;
    }

    public int SetGrenade(int amount)
    {
        int accepted = AddClamped(ref grenadeCount, grenadeMax, amount, OnGrenadeFullChanged);
        if (accepted > 0) explosives?.AddGrenades(accepted);
        return accepted;
    }

    public int SetLandmine(int amount)
    {
        int accepted = AddClamped(ref landmineCount, landmineMax, amount, OnLandmineFullChanged);
        if (accepted > 0) explosives?.AddLandmines(accepted);
        return accepted;
    }

    public int SetMedkit(int amount) => AddClamped(ref medkitCount, medkitMax, amount, OnMedkitFullChanged);

    public int SetBandage(int amount)
    {
        int accepted = AddClamped(ref bandageCount, bandageMax, amount, OnBandageFullChanged);
        if (accepted > 0) switchWeapons?.AddBandage(accepted);
        return accepted;
    }

    public int SetSilencer(int amount) => AddClamped(ref silencerCount, silencerMax, amount, OnSilencerFullChanged);

    public int SetAlcohol(int amount)
    {
        int accepted = AddClamped(ref alcoholCount, alcoholMax, amount, OnAlcoholFullChanged);
        if (accepted > 0) inventoryHandler?.AddAlcohol(accepted);
        return accepted;
    }

    public int SetRag(int amount)
    {
        int accepted = AddClamped(ref ragCount, ragMax, amount, OnRagFullChanged);
        if (accepted > 0) inventoryHandler?.AddRag(accepted);
        return accepted;
    }

    public int SetBinding(int amount)
    {
        int accepted = AddClamped(ref bindingCount, bindingMax, amount, OnBindingFullChanged);
        if (accepted > 0) inventoryHandler?.AddBinding(accepted);
        return accepted;
    }

    public int SetGunpowder(int amount)
    {
        int accepted = AddClamped(ref gunpowderCount, gunpowderMax, amount, OnGunpowderFullChanged);
        if (accepted > 0) inventoryHandler?.AddGunPowder(accepted);
        return accepted;
    }

    public int SetCan(int amount)
    {
        int accepted = AddClamped(ref canCount, canMax, amount, OnCanFullChanged);
        if (accepted > 0) inventoryHandler?.AddCan(accepted);
        return accepted;
    }

    // ---------- consume APIs ----------
    public bool ConsumePistolAmmo(int amount) => ConsumeClamped(ref pistolAmmoCount, pistolAmmoMax, amount, OnPistolAmmoFullChanged);
    public bool ConsumeShotgunAmmo(int amount) => ConsumeClamped(ref shotgunAmmoCount, shotgunAmmoMax, amount, OnShotgunAmmoFullChanged);
    public bool ConsumeSniperAmmo(int amount) => ConsumeClamped(ref sniperAmmoCount, sniperAmmoMax, amount, OnSniperAmmoFullChanged);
    public bool ConsumeGrenade(int amount) => ConsumeClamped(ref grenadeCount, grenadeMax, amount, OnGrenadeFullChanged);
    public bool ConsumeLandmine(int amount) => ConsumeClamped(ref landmineCount, landmineMax, amount, OnLandmineFullChanged);
    public bool ConsumeMedkit(int amount) => ConsumeClamped(ref medkitCount, medkitMax, amount, OnMedkitFullChanged);
    public bool ConsumeBandage(int amount) => ConsumeClamped(ref bandageCount, bandageMax, amount, OnBandageFullChanged);
    public bool ConsumeSilencer(int amount) => ConsumeClamped(ref silencerCount, silencerMax, amount, OnSilencerFullChanged);
    public bool ConsumeAlcohol(int amount) => ConsumeClamped(ref alcoholCount, alcoholMax, amount, OnAlcoholFullChanged);
    public bool ConsumeRag(int amount) => ConsumeClamped(ref ragCount, ragMax, amount, OnRagFullChanged);
    public bool ConsumeBinding(int amount) => ConsumeClamped(ref bindingCount, bindingMax, amount, OnBindingFullChanged);
    public bool ConsumeGunpowder(int amount) => ConsumeClamped(ref gunpowderCount, gunpowderMax, amount, OnGunpowderFullChanged);
    public bool ConsumeCan(int amount) => ConsumeClamped(ref canCount, canMax, amount, OnCanFullChanged);

    // getters
    public int PistolAmmoCount => pistolAmmoCount;
    public int PistolAmmoMax => pistolAmmoMax;
    public int AlcoholCount => alcoholCount;
    public int RagCount => ragCount;
    public int BindingCount => bindingCount;
    public int GunpowderCount => gunpowderCount;
    public int CanCount => canCount;

    private void ClampAll()
    {
        pistolAmmoCount = Mathf.Clamp(pistolAmmoCount, 0, pistolAmmoMax);
        shotgunAmmoCount = Mathf.Clamp(shotgunAmmoCount, 0, shotgunAmmoMax);
        sniperAmmoCount = Mathf.Clamp(sniperAmmoCount, 0, sniperAmmoMax);
        grenadeCount = Mathf.Clamp(grenadeCount, 0, grenadeMax);
        landmineCount = Mathf.Clamp(landmineCount, 0, landmineMax);
        medkitCount = Mathf.Clamp(medkitCount, 0, medkitMax);
        bandageCount = Mathf.Clamp(bandageCount, 0, bandageMax);
        silencerCount = Mathf.Clamp(silencerCount, 0, silencerMax);
        alcoholCount = Mathf.Clamp(alcoholCount, 0, alcoholMax);
        ragCount = Mathf.Clamp(ragCount, 0, ragMax);
        bindingCount = Mathf.Clamp(bindingCount, 0, bindingMax);
        gunpowderCount = Mathf.Clamp(gunpowderCount, 0, gunpowderMax);
        canCount = Mathf.Clamp(canCount, 0, canMax);
    }

    private void BroadcastAllFullStates()
    {
        OnPistolAmmoFullChanged?.Invoke(pistolAmmoCount >= pistolAmmoMax);
        OnShotgunAmmoFullChanged?.Invoke(shotgunAmmoCount >= shotgunAmmoMax);
        OnSniperAmmoFullChanged?.Invoke(sniperAmmoCount >= sniperAmmoMax);
        OnGrenadeFullChanged?.Invoke(grenadeCount >= grenadeMax);
        OnLandmineFullChanged?.Invoke(landmineCount >= landmineMax);
        OnMedkitFullChanged?.Invoke(medkitCount >= medkitMax);
        OnBandageFullChanged?.Invoke(bandageCount >= bandageMax);
        OnSilencerFullChanged?.Invoke(silencerCount >= silencerMax);
        OnAlcoholFullChanged?.Invoke(alcoholCount >= alcoholMax);
        OnRagFullChanged?.Invoke(ragCount >= ragMax);
        OnBindingFullChanged?.Invoke(bindingCount >= bindingMax);
        OnGunpowderFullChanged?.Invoke(gunpowderCount >= gunpowderMax);
        OnCanFullChanged?.Invoke(canCount >= canMax);
    }
}