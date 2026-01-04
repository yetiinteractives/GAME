using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;


public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected int magCapacity = 10;     // Magazine capacity
    [SerializeField] protected int totalBullet = 100;    // Total bullets in pocket/reserve
    [SerializeField] protected float reloadTime = 1.5f;
    [SerializeField] protected float fireRate = 0.3f;
    [SerializeField] protected float recoilIntensity= 1.5f;
    [SerializeField] protected float recoilDuration= 0.5f;

    [Header("Audio and Visuals")]
    [SerializeField] protected AudioClip shootSound;
    [SerializeField] protected AudioClip reloadSound;
    [SerializeField] protected ParticleSystem muzzleFlash;
    [SerializeField] protected GameObject bulletImpactPrefab;
    [SerializeField] protected GameObject bulleteHolePrefab;
    protected AudioSource audioSource; 

    // Events
    public event Action<int, int> OnAmmoChanged; // bulletOnMag, totalBullet
    public event Action<string> OnWeaponStatusChanged;

    // Variables
    protected int bulletOnMag;
    protected bool isReloading = false;
    protected bool isAiming = false;
    protected float nextFireTime = 0f;

    // Properties
    public bool CanShoot => bulletOnMag > 0 && !isReloading && isAiming && Time.time >= nextFireTime;
    public bool IsReloading => isReloading;
    public int BulletOnMag => bulletOnMag;
    public int MagCapacity => magCapacity;
    public int TotalBullet => totalBullet;

    protected virtual void Awake()
    {
        bulletOnMag = magCapacity;
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Stop muzzle flash on start if it exists
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop();
        }
    }

    protected virtual void OnEnable()
    {
        MousePosition3D.OnFirePerformed += OnFireInput;
        UpdateStatus("Ready");

        // Stop muzzle flash when enabled
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop();
        }

        
    }

    protected virtual void OnDisable()
    {
        MousePosition3D.OnFirePerformed -= OnFireInput;
        if (isReloading)
        {
            StopAllCoroutines();
            isReloading = false;
        }

        // Stop muzzle flash when disabled
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop();
        }
    }

    void OnFireInput(RaycastHit hit)
    {
        // Only shoot if we can shoot and aiming
        if (isAiming && CanShoot)
        {
            Shoot(hit);
        }
        else if (isAiming && bulletOnMag == 0 && !isReloading)
        {
            UpdateStatus("Out of Ammo!");
            StartReload();
        }
    }

    protected virtual void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        // AIM - Right click hold
        if (Input.GetMouseButtonDown(1))
        {
            StartAiming();
        }
        if (Input.GetMouseButtonUp(1))
        {
            StopAiming();
        }

        // RELOAD on R key
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && bulletOnMag < magCapacity && totalBullet > 0)
        {
            StartReload();
        }

        if(isAiming)
        {
            ScopeCheck();
        }
    }

    protected virtual void StartAiming()
    {
        isAiming = true;
        UpdateStatus("Aiming");
    }

    protected virtual void StopAiming()
    {
        isAiming = false;
        UpdateStatus("Ready");
    }

    protected virtual void Shoot(RaycastHit hit)
    {
        // Reduce bullet in magazine
        bulletOnMag--;
        nextFireTime = Time.time + fireRate;

        // Update ammo UI
        OnAmmoChanged?.Invoke(bulletOnMag, totalBullet);

        // Play shoot sound
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Handle muzzle flash with coroutine
        StartCoroutine(PlayMuzzleFlash());

        // Instantiate bullet impact effect at hit point
        if (bulletImpactPrefab != null)
        {
            GameObject impact = Instantiate(bulletImpactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impact, 2f);
        }
        // Instantiate bullet tracer effect
        if (bulleteHolePrefab != null)
        {
            GameObject hole = Instantiate(bulleteHolePrefab, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(-hit.normal));
            Destroy(hole, 30f);
        }
        
        CinemachineShake.Instance.Shake(recoilIntensity, recoilDuration); // Camera shake



        // Call derived class shooting logic
        OnShoot(hit);

        // Check ammo
        if (bulletOnMag <= 0)
        {
            UpdateStatus("Out of Ammo!");
        }
        else
        {
            UpdateStatus("Aiming");
        }

        Debug.Log($"{gameObject.name} fired! Mag: {bulletOnMag}/{magCapacity}, Total: {totalBullet}");
    }

    // Coroutine to play muzzle flash
    protected virtual IEnumerator PlayMuzzleFlash()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
            yield return new WaitForSeconds(0.1f); // Flash for 0.1 seconds
            muzzleFlash.Stop();
        }
    }

    // Virtual method for derived classes to override for specific shooting logic
    protected virtual void OnShoot(RaycastHit hit)
    {
        // Override in derived classes for specific weapon behavior
    }

    public void StartReload()
    {
        if (!isReloading && bulletOnMag < magCapacity && totalBullet > 0)
        {
            StartCoroutine(Reload());
        }
    }

    protected virtual IEnumerator Reload()
    {
        isReloading = true;
        UpdateStatus("Reloading...");

        // Play reload sound
        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(reloadTime);

        // Calculate how many bullets to reload
        int bulletsNeeded = magCapacity - bulletOnMag;
        int bulletsToAdd = Mathf.Min(bulletsNeeded, totalBullet);

        // Update bullet counts
        bulletOnMag += bulletsToAdd;
        totalBullet -= bulletsToAdd;

        isReloading = false;
        OnAmmoChanged?.Invoke(bulletOnMag, totalBullet);

        if (isAiming)
        {
            UpdateStatus("Aiming");
        }
        else
        {
            UpdateStatus("Ready");
        }

        Debug.Log($"{gameObject.name} reloaded! Added {bulletsToAdd} bullets");
    }

    protected void UpdateStatus(string status)
    {
        OnWeaponStatusChanged?.Invoke(status);
    }

    // Add bullets to reserve
    public void AddBullets(int amount)
    {
        totalBullet += amount;
        OnAmmoChanged?.Invoke(bulletOnMag, totalBullet);
    }

    protected virtual void ScopeCheck()
    {
        //to override in sniper class
    }





}