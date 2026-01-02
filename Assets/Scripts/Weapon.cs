using System;
using System.Collections;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected int magazineSize = 10;
    [SerializeField] protected float reloadTime = 1.5f;
    [SerializeField] protected float fireRate = 0.3f;

    [Header("Effects")]
    [SerializeField] protected ParticleSystem muzzleFlash;
    [SerializeField] protected ParticleSystem bulletImpact;

    [Header("Audio")]
    [SerializeField] protected AudioClip shootSound;
    [SerializeField] protected AudioClip reloadSound;
    protected AudioSource audioSource;

    // Events
    public event Action<int> OnAmmoChanged;
    public event Action<string> OnWeaponStatusChanged;

    // Variables
    protected int currentAmmo;
    protected bool isReloading = false;
    protected float nextFireTime = 0f;

    // Properties
    public bool CanShoot => currentAmmo > 0 && !isReloading && Time.time >= nextFireTime;
    public bool IsReloading => isReloading;
    public int CurrentAmmo => currentAmmo;
    public int MagazineSize => magazineSize;

    protected virtual void Awake()
    {
        currentAmmo = magazineSize;
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError($"No AudioSource found on {gameObject.name}");
        }

        // Stop muzzle flash on start
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    protected virtual void OnEnable()
    {
        MousePosition3D.OnFirePerformed += OnFireInput;
        UpdateStatus("Ready");
    }

    protected virtual void OnDisable()
    {
        MousePosition3D.OnFirePerformed -= OnFireInput;

        if (isReloading)
        {
            StopAllCoroutines();
            isReloading = false;
        }

        // Stop muzzle flash
        StopMuzzleFlash();
    }

    private void OnFireInput(RaycastHit hit)
    {
        if (!gameObject.activeInHierarchy) return;

        // SHOOT on left click
        if (Input.GetMouseButtonDown(0))
        {
            if (CanShoot)
            {
                Shoot(hit);
            }
            else if (currentAmmo == 0 && !isReloading)
            {
                Debug.Log("Out of ammo!");
                UpdateStatus("Out of Ammo!");
                StartReload();
            }
        }

        // RELOAD on R key
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < magazineSize)
        {
            StartReload();
        }
    }

    protected virtual void Shoot(RaycastHit hit)
    {
        // Reduce ammo
        currentAmmo--;
        nextFireTime = Time.time + fireRate;

        // Update ammo UI
        OnAmmoChanged?.Invoke(currentAmmo);

        // Muzzle flash with coroutine
        StartCoroutine(PlayMuzzleFlash());

        // Play shoot sound
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Show bullet impact
        if (bulletImpact != null)
        {
            bulletImpact.transform.position = hit.point;
            bulletImpact.Play();
        }

        // Check ammo
        if (currentAmmo <= 0)
        {
            UpdateStatus("Out of Ammo!");
        }
        else
        {
            UpdateStatus("Ready");
        }

        Debug.Log($"{gameObject.name} fired! Ammo: {currentAmmo}/{magazineSize}");
    }

    protected IEnumerator PlayMuzzleFlash()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
            yield return new WaitForSeconds(0.1f); // Flash duration
            StopMuzzleFlash();
        }
    }

    protected void StopMuzzleFlash()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void StartReload()
    {
        if (!isReloading && currentAmmo < magazineSize)
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

        // Refill ammo
        currentAmmo = magazineSize;
        isReloading = false;
        OnAmmoChanged?.Invoke(currentAmmo);
        UpdateStatus("Ready");

        Debug.Log($"{gameObject.name} reloaded!");
    }

    protected void UpdateStatus(string status)
    {
        OnWeaponStatusChanged?.Invoke(status);
        Debug.Log($"Weapon Status: {status}");
    }
}