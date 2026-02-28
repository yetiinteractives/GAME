using System;
using System.Collections;
using UnityEngine;
using static ImpactManager;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] protected int damage = 10;
    [SerializeField] protected float force = 5f;
    [SerializeField] protected int magCapacity = 10;
    [SerializeField] protected int totalBullet = 100;
    [SerializeField] protected float reloadTime = 1.5f;
    [SerializeField] protected float fireRate = 0.3f;
    [SerializeField] protected float recoilIntensity = 1.5f;
    [SerializeField] protected float recoilDuration = 0.5f;

    [Header("Audio and Visuals")]
    [SerializeField] protected AudioClip shootSound;
    [SerializeField] protected AudioClip reloadSound;
    [SerializeField] protected AudioClip emptyMagSound;
    [SerializeField] protected ParticleSystem muzzleFlash;
    [SerializeField] protected Light muzzleFlashLight;

    [Header("AI Sound Emission")]
    [Tooltip("How far enemies can hear this weapon fire.")]
    [SerializeField] protected float gunshotLoudness = 40f;
    [Tooltip("How far enemies can hear a reload.")]
    [SerializeField] protected float reloadLoudness = 8f;

    protected AudioSource audioSource;
    protected FreeLookADS freeLookAds;

    // Events
    public event Action<int, int> OnAmmoChanged; // bulletOnMag, totalBullet
    public event Action<string> OnWeaponStatusChanged;
    public static event Action OnBulletShot;

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

    [SerializeField] protected GunTypeEnum gunType;

    protected virtual void Awake()
    {
        bulletOnMag = magCapacity;
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (muzzleFlash != null)
            muzzleFlash.Stop();

        freeLookAds = FindFirstObjectByType<FreeLookADS>();
    }

    protected virtual void OnEnable()
    {
        MousePosition3D.OnFirePerformed += OnFireInput;

        
        OnAmmoChanged?.Invoke(bulletOnMag, totalBullet);

        UpdateStatus("Ready");

        if (muzzleFlash != null)
            muzzleFlash.Stop();
        muzzleFlashLight.gameObject.SetActive(true);
        muzzleFlashLight.enabled = false;
    }

    protected virtual void OnDisable()
    {
        MousePosition3D.OnFirePerformed -= OnFireInput;

        if (isReloading)
        {
            StopAllCoroutines();
            isReloading = false;
        }

        if (muzzleFlash != null)
            muzzleFlash.Stop();
    }

    void OnFireInput(RaycastHit hit)
    {
        if (isAiming && CanShoot)
        {
            Shoot(hit);
        }
        else if (isAiming && bulletOnMag == 0 && !isReloading)
        {
            UpdateStatus("Out of Ammo!");
            audioSource.PlayOneShot(emptyMagSound);
        }
    }

    protected virtual void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        // AIM - Right click hold
        if (Input.GetMouseButtonDown(1)) StartAiming();
        if (Input.GetMouseButtonUp(1)) StopAiming();

        // RELOAD on R key
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && bulletOnMag < magCapacity && totalBullet > 0)
            StartReload();

        if (isAiming)
            ScopeCheck();
    }

    protected virtual void StartAiming()
    {
        isAiming = true;
        UpdateStatus("Aiming");
        freeLookAds.SetADSState();
    }

    protected virtual void StopAiming()
    {
        isAiming = false;
        UpdateStatus("Ready");
        freeLookAds.SetNormalState();
    }

    protected virtual void Shoot(RaycastHit hit)
    {
        ApplyDamage(hit, damage);

        bulletOnMag--;
        nextFireTime = Time.time + fireRate;

        OnAmmoChanged?.Invoke(bulletOnMag, totalBullet);
        OnBulletShot?.Invoke();

        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound);

        // Notify AI sound system
        SoundEmitter.EmitSoundAt(transform.position, SoundType.Gunshot, gunshotLoudness, gameObject);

        if (muzzleFlash != null)
            StartCoroutine(PlayMuzzleFlash());

        if (ImpactManager.Instance != null)
            ImpactManager.Instance.SpawnImpact(gunType, hit);

        CinemachineShake.Instance.Shake(recoilIntensity, recoilDuration);

        OnShoot(hit);

        if (bulletOnMag <= 0)
            UpdateStatus("Out of Ammo!");
        else
            UpdateStatus("Aiming");

        Debug.Log($"{gameObject.name} fired! Mag: {bulletOnMag}/{magCapacity}, Total: {totalBullet}");
    }

    protected void ApplyDamage(RaycastHit hit, int damageAmount)
    {
        IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
        if (damageable == null) return;

        float multiplier = 1f;

        EnemyBodyType bodyType = hit.collider.GetComponent<EnemyBodyType>();
        if (bodyType != null)
            multiplier = bodyType.DamageMultiplyer();

        float finalDamage = damageAmount * multiplier;

        // Apply damage first
        damageable.TakeDamage(finalDamage);

        // If enemy died from this shot -> apply death force to the limb that was hit
        if (damageable.IsDead())
        {
            Vector3 direction = -hit.normal;
            Vector3 impulse = direction.normalized * force * multiplier;

            damageable.ApplyDeathForce(hit.collider, hit.point, impulse);
        }
    }

    protected virtual IEnumerator PlayMuzzleFlash()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
            muzzleFlashLight.enabled = true;
            yield return new WaitForSeconds(0.04f);
            muzzleFlashLight.enabled = false;

            yield return new WaitForSeconds(0.06f);
            muzzleFlash.Stop();
        }
    }

    protected virtual void OnShoot(RaycastHit hit)
    {
        // Override in child classes
    }

    public void StartReload()
    {
        if (!isReloading && bulletOnMag < magCapacity && totalBullet > 0)
        {
            StartCoroutine(Reload());
            ReloadHandler.Instance.HandleReload();
        }
    }

    protected virtual IEnumerator Reload()
    {
        isReloading = true;
        UpdateStatus("Reloading...");

        if (audioSource != null && reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

        // Notify AI sound system
        SoundEmitter.EmitSoundAt(transform.position, SoundType.Reload, reloadLoudness, gameObject);

        yield return new WaitForSeconds(reloadTime);

        int bulletsNeeded = magCapacity - bulletOnMag;
        int bulletsToAdd = Mathf.Min(bulletsNeeded, totalBullet);

        bulletOnMag += bulletsToAdd;
        totalBullet -= bulletsToAdd;

        isReloading = false;
        OnAmmoChanged?.Invoke(bulletOnMag, totalBullet);

        UpdateStatus(isAiming ? "Aiming" : "Ready");

        Debug.Log($"{gameObject.name} reloaded! Added {bulletsToAdd} bullets");
    }

    protected void UpdateStatus(string status)
    {
        OnWeaponStatusChanged?.Invoke(status);
    }

    public void AddBullets(int amount)
    {
        totalBullet += amount;
        OnAmmoChanged?.Invoke(bulletOnMag, totalBullet);
    }

    protected virtual void ScopeCheck()
    {
        // Override in sniper class
    }
}