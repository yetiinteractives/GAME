using System;
using System.Collections;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected int magazineSize = 10;
    [SerializeField] protected float reloadTime = 1.5f;
    [SerializeField] protected float fireRate = 0.5f;
    [SerializeField] protected float effectiveRange = 50f;
    [SerializeField] protected float maxRange = 100f;

    [Header("Visual/Audio")]
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform muzzleTransform;
    [SerializeField] protected AudioClip fireSound;
    [SerializeField] protected AudioClip reloadSound;
    [SerializeField] protected AudioClip emptySound;
    [SerializeField] protected AudioSource audioSource;

    // C# Events (instead of UnityEvents)
    public event Action OnWeaponFired;
    public event Action OnReloadStarted;
    public event Action OnReloadCompleted;
    public event Action<int> OnAmmoChanged; // Pass current ammo count

    // Protected variables
    protected int currentAmmo;
    protected bool isReloading = false;
    protected float nextFireTime = 0f;
    protected int weaponIndex;

    // Properties
    public bool CanShoot => currentAmmo > 0 && !isReloading && Time.time >= nextFireTime;
    public bool IsReloading => isReloading;
    public int CurrentAmmo => currentAmmo;
    public int MagazineSize => magazineSize;
    public float Damage => baseDamage;
    public float EffectiveRange => effectiveRange;
    public float MaxRange => maxRange;

    protected virtual void Awake()
    {
        currentAmmo = magazineSize;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    protected virtual void OnEnable()
    {
        // Subscribe to events
        SwitchWeapons.OnWeaponSwitch += HandleWeaponSwitch;
        MousePosition3D.OnFirePerformed += HandleFire;
    }

    protected virtual void OnDisable()
    {
        // Unsubscribe from events
        SwitchWeapons.OnWeaponSwitch -= HandleWeaponSwitch;
        MousePosition3D.OnFirePerformed -= HandleFire;

        // Stop any reload in progress
        if (isReloading)
        {
            StopAllCoroutines();
            isReloading = false;
        }
    }

    private void HandleWeaponSwitch(int switchedIndex)
    {
        // This weapon is active if its index matches the switched index
        bool isActive = (switchedIndex == weaponIndex);

        // Auto-reload if empty when switching to this weapon
        if (isActive && !isReloading && currentAmmo <= 0)
        {
            StartReload();
        }
    }

    private void HandleFire(RaycastHit hit)
    {
        // Only process if this weapon is active
        if (!gameObject.activeInHierarchy) return;

        // Check for shooting input (left click)
        if (Input.GetMouseButtonDown(0))
        {
            if (CanShoot)
            {
                // Calculate damage based on distance
                float damage = CalculateDamageBasedOnDistance(hit.distance);
                ProcessShot(hit, damage);
            }
            else if (currentAmmo <= 0 && !isReloading)
            {
                // Play empty sound
                PlaySound(emptySound);
                TryAutoReload();
            }
        }

        // Check for reload input (R key)
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < magazineSize)
        {
            StartReload();
        }
    }

    protected virtual void ProcessShot(RaycastHit hit, float calculatedDamage)
    {
        // Reduce ammo and set cooldown
        currentAmmo--;
        nextFireTime = Time.time + fireRate;

        // Invoke ammo changed event
        OnAmmoChanged?.Invoke(currentAmmo);

        // Play fire sound
        PlaySound(fireSound);

        // Create visual bullet effect (optional)
        if (bulletPrefab && muzzleTransform)
        {
            CreateBulletEffect(hit.point);
        }

        // Apply damage to target if it has a Health component
        if (hit.collider != null)
        {
            Health targetHealth = hit.collider.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(calculatedDamage);
            }
        }

        // Invoke weapon fired event
        OnWeaponFired?.Invoke();
    }

    protected virtual float CalculateDamageBasedOnDistance(float hitDistance)
    {
        // Base implementation: linear damage falloff
        if (hitDistance <= effectiveRange)
        {
            return baseDamage;
        }
        else if (hitDistance >= maxRange)
        {
            return baseDamage * 0.1f; // 10% damage at max range
        }
        else
        {
            // Linear falloff between effective range and max range
            float falloffPercent = (hitDistance - effectiveRange) / (maxRange - effectiveRange);
            return baseDamage * Mathf.Lerp(1f, 0.1f, falloffPercent);
        }
    }

    public void StartReload()
    {
        if (!isReloading && currentAmmo < magazineSize)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    protected virtual IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        OnReloadStarted?.Invoke();

        // Play reload sound
        PlaySound(reloadSound);

        float elapsedTime = 0f;

        while (elapsedTime < reloadTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentAmmo = magazineSize;
        isReloading = false;
        OnAmmoChanged?.Invoke(currentAmmo);
        OnReloadCompleted?.Invoke();
    }

    protected void TryAutoReload()
    {
        // Auto-reload after a short delay when trying to fire empty weapon
        if (!isReloading && currentAmmo == 0)
        {
            Invoke(nameof(StartReload), 0.2f);
        }
    }

    protected virtual void CreateBulletEffect(Vector3 hitPoint)
    {
        // Instantiate bullet visual that travels to hit point
        GameObject bullet = Instantiate(bulletPrefab, muzzleTransform.position, Quaternion.identity);
        StartCoroutine(MoveBulletTowardsTarget(bullet, hitPoint));
    }

    protected IEnumerator MoveBulletTowardsTarget(GameObject bullet, Vector3 targetPosition)
    {
        float travelTime = 0.1f; // Bullet travel time
        float elapsedTime = 0f;
        Vector3 startPosition = bullet.transform.position;

        while (elapsedTime < travelTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / travelTime;
            bullet.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        Destroy(bullet);
    }

    protected void PlaySound(AudioClip clip)
    {
        if (audioSource && clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Called by child classes to set their index
    protected void SetWeaponIndex(int index)
    {
        weaponIndex = index;
    }
}