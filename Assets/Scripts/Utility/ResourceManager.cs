using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [SerializeField] private Pistol pistol;
    [SerializeField] private Shotgun shotgun;
    [SerializeField] private Sniper sniper;
    [SerializeField] private ExplosivesHandler explosives;
    [SerializeField] private SwitchWeapons switchWeapons;

    private void Awake()
    {
        // If an instance already exists, destroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance
        Instance = this;

        // persist between scenes
        DontDestroyOnLoad(gameObject);
    }

    public void SetPistolAmmo(int ammo)
    {
        pistol.AddBullets(ammo);
    }
    public void SetShotgunAmmo(int ammo)
    {
        shotgun.AddBullets(ammo);
    }
    public void SetSniperAmmo(int ammo)
    {
        sniper.AddBullets(ammo);
    }
    public void SetGrenade(int amount)
    {
        explosives.AddGrenades(amount);
    }
    public void SetLandmine(int amount)
    {
        explosives.AddLandmines(amount);
    }

    public void SetMedkit(int amount)
    {
        // Implement medkit logic here
    }
    public void SetBandage(int amount)
    {
        switchWeapons.AddBandage(amount);
    }
    public void SetSilencer(int amount)
    {
        // Implement silencer logic here
    }
    public void SetAlchohol(int amount)
    {
        // Implement alchohol logic here
    }
    public void SetRag(int amount)
    {
        // Implement rag logic here
    }
    public void SetBinding(int amount)
    {
        // Implement binding logic here
    }public void SetGunpowder(int amount)
    {
        // Implement gunpowder logic here  
    }
    public void SetCan(int amount)
    {
        // Implement can logic here
    }


    }