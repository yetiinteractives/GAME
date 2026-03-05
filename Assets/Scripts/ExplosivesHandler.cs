using System;
using UnityEngine;

public class ExplosivesHandler : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] private GrenadeThrow grenade;
    [SerializeField] private LandminePlacement landmine;
    [SerializeField] private GameObject grenadeModel;
    [SerializeField] private GameObject landmineModel;

    [Header("Resource Settings")]
    [SerializeField] private int initialGrenadeCount;
    [SerializeField] private int initialLandmineCount;
    [SerializeField] private int maxGrenade;
    [SerializeField] private int maxLandmine;

    private int currentGrenadeCount;
    private int currentLandmineCount;
    private bool hasGrenades;
    private bool hasLandmine;

    private int currentWeaponIndex;

    private void Start()
    {
        

        currentGrenadeCount = initialGrenadeCount;
        currentLandmineCount = initialLandmineCount;

        if(grenade == null)
        {
            grenade = GetComponentInChildren<GrenadeThrow>();
        }
        if(landmine == null)
        {
            landmine = GetComponentInChildren<LandminePlacement>();
        }
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
        grenade.gameObject.SetActive(isTrue);
        grenadeModel.SetActive(isTrue);
    }
    private void ToggleLandmine(bool isTrue)
    {
            landmine.gameObject.SetActive(isTrue);
            landmineModel.SetActive(isTrue);
    }


    public void ConsumeGrenade()
    {
       
        if(!hasGrenades) return;


        currentGrenadeCount--;
        CheckIfHasExplosives();

        ToggleGrenade(hasGrenades && currentWeaponIndex == 4);


    }
    public void ConsumeLandmine()
    {
        if(!hasLandmine) return;

            currentLandmineCount--;
            CheckIfHasExplosives();

        ToggleLandmine(hasLandmine && currentWeaponIndex == 5 );


    }
     public void AddGrenades(int amount)
    {
        currentGrenadeCount = Mathf.Clamp(currentGrenadeCount + amount, 0, maxGrenade);
        CheckIfHasExplosives();

        ToggleGrenade(hasGrenades && currentWeaponIndex == 4);
    }
    public void AddLandmines(int amount)
    {
        currentLandmineCount = Mathf.Clamp(currentLandmineCount + amount, 0, maxLandmine);
        CheckIfHasExplosives();

        ToggleLandmine(hasLandmine && currentWeaponIndex == 5);
    }


     private void OnDestroy()
    {
        SwitchWeapons.OnWeaponSwitch -= HandleWeaponSwitch;
    }

}
