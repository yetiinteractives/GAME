using System;
using UnityEngine;

public class ExplosivesHandler : MonoBehaviour
{

    public event Action<int> OnExplosivesCountChanged; 

    [Header("Refrences")]
    [SerializeField] private GrenadeThrow grenade;
    [SerializeField] private LandminePlacement landmine;
    [SerializeField] private GameObject grenadeModel;
    [SerializeField] private GameObject landmineModel;

    [Header("Resource Settings")]
    private int initialGrenadeCount;
    private int initialLandmineCount;
    [SerializeField] private int maxGrenade;
    [SerializeField] private int maxLandmine;

    private int currentGrenadeCount;
    private int currentLandmineCount;
    private bool hasGrenades;
    private bool hasLandmine;

    private int currentWeaponIndex;

    private void Start()
    {
        if (grenade == null) grenade = GetComponentInChildren<GrenadeThrow>(true);
        if (landmine == null) landmine = GetComponentInChildren<LandminePlacement>(true);

        // IMPORTANT: initialize from manager, not initial fields
        if (ResourceManager.Instance != null)
            SetCounts(ResourceManager.Instance.GrenadeCount, ResourceManager.Instance.LandmineCount);
        else
            SetCounts(initialGrenadeCount, initialLandmineCount);
    }

    private void OnEnable()
    {
        SwitchWeapons.OnWeaponSwitch += HandleWeaponSwitch;
    }

    private void HandleWeaponSwitch(int index)
    {
       currentWeaponIndex = index;
        CheckIfHasExplosives();

        ToggleGrenade(hasGrenades && currentWeaponIndex == 4);
        ToggleLandmine(hasLandmine && currentWeaponIndex == 5);
       

    }
    private void CheckIfHasExplosives()
    {
        hasGrenades = (currentGrenadeCount > 0) ;
        hasLandmine = (currentLandmineCount > 0);   
    }



    private void ToggleGrenade(bool isTrue)
    {
        if (grenade != null) grenade.gameObject.SetActive(isTrue);
        if (grenadeModel != null) grenadeModel.SetActive(isTrue);
        if (isTrue) OnExplosivesCountChanged?.Invoke(currentGrenadeCount);
    }
    private void ToggleLandmine(bool isTrue)
    {
        if (landmine != null) landmine.gameObject.SetActive(isTrue);
        if (landmineModel != null) landmineModel.SetActive(isTrue);
        if (isTrue) OnExplosivesCountChanged?.Invoke(currentLandmineCount);
    }

    public void ConsumeGrenade()
    {
        if (!hasGrenades || ResourceManager.Instance == null) return;

        currentGrenadeCount = Mathf.Max(0, currentGrenadeCount - 1);
        ResourceManager.Instance.SetGrenadeAbsolute(currentGrenadeCount);

        CheckIfHasExplosives();
        ToggleGrenade(hasGrenades && currentWeaponIndex == 4);
        OnExplosivesCountChanged?.Invoke(currentGrenadeCount);

        InventoryHandler.Instance?.SyncFromResourceManagerForUI();
    }
    public void ConsumeLandmine()
    {
        if (!hasLandmine || ResourceManager.Instance == null) return;

        currentLandmineCount = Mathf.Max(0, currentLandmineCount - 1);
        ResourceManager.Instance.SetLandmineAbsolute(currentLandmineCount);

        CheckIfHasExplosives();
        ToggleLandmine(hasLandmine && currentWeaponIndex == 5);
        OnExplosivesCountChanged?.Invoke(currentLandmineCount);

        InventoryHandler.Instance?.SyncFromResourceManagerForUI();
    }
    public void AddGrenades(int amount)
    {
        currentGrenadeCount = Mathf.Clamp(currentGrenadeCount + amount, 0, maxGrenade);
        CheckIfHasExplosives();

        ToggleGrenade(hasGrenades && currentWeaponIndex == 4);

        OnExplosivesCountChanged?.Invoke(currentGrenadeCount);
    }
    public void AddLandmines(int amount)
    {
        currentLandmineCount = Mathf.Clamp(currentLandmineCount + amount, 0, maxLandmine);
        CheckIfHasExplosives();

        ToggleLandmine(hasLandmine && currentWeaponIndex == 5);

        OnExplosivesCountChanged?.Invoke(currentLandmineCount);
    }

    public void SetCounts(int grenades, int landmines)
    {
        currentGrenadeCount = Mathf.Clamp(grenades, 0, maxGrenade);
        currentLandmineCount = Mathf.Clamp(landmines, 0, maxLandmine);

        CheckIfHasExplosives();

        ToggleGrenade(hasGrenades && currentWeaponIndex == 4);
        ToggleLandmine(hasLandmine && currentWeaponIndex == 5);

        if (currentWeaponIndex == 4) OnExplosivesCountChanged?.Invoke(currentGrenadeCount);
        if (currentWeaponIndex == 5) OnExplosivesCountChanged?.Invoke(currentLandmineCount);
    }

    private void OnDestroy()
    {
        SwitchWeapons.OnWeaponSwitch -= HandleWeaponSwitch;
    }

}
