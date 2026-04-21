using UnityEngine;
using UnityEngine.UI;

public class Pistol : Weapon
{
    [Header("Silencer")]
    [SerializeField] private int silencerDurability = 5;
    [SerializeField] private GameObject silencerModel;
    [SerializeField] private AudioClip silencerSound;

    [Header("UI")]
    [SerializeField] private Image silencerAttachedIcon;
    [SerializeField] private Image silencerDetachedIcon;
    [SerializeField] private Slider silencerDurabilitySlider;

    [Header("Sound")]
    [SerializeField, Range(0.01f, 1f)]
    private float silencedLoudnessMultiplier = 0.1f;

    private bool isSilencerOn = false;
    private int currentSilencerDurability;

    private float baseGunshotLoudness;
    private AudioClip currentShootSound;

    protected override void Awake()
    {
        base.Awake();

        baseGunshotLoudness = gunshotLoudness;
        currentShootSound = shootSound;

        if (silencerDurabilitySlider != null)
            silencerDurabilitySlider.maxValue = silencerDurability;

        // Load persisted runtime state
        if (ResourceManager.Instance != null)
        {
            isSilencerOn = ResourceManager.Instance.IsPistolSilencerEquipped;
            currentSilencerDurability = ResourceManager.Instance.PistolSilencerDurability;
        }
        else
        {
            isSilencerOn = false;
            currentSilencerDurability = silencerDurability;
        }

        if (currentSilencerDurability <= 0)
            currentSilencerDurability = silencerDurability;

        ApplySilencerStateVisualAndSound();
        UpdateSilencerUI();
        SaveSilencerRuntimeState();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (ResourceManager.Instance != null)
        {
            isSilencerOn = ResourceManager.Instance.IsPistolSilencerEquipped;
            currentSilencerDurability = ResourceManager.Instance.PistolSilencerDurability;

            if (currentSilencerDurability <= 0 && ResourceManager.Instance.SilencerCount > 0)
                currentSilencerDurability = silencerDurability;
        }

        ApplySilencerStateVisualAndSound();
        UpdateSilencerUI();
    }

    private void DisableAllSilencerIcons()
    {
        if (silencerDetachedIcon != null) silencerDetachedIcon.gameObject.SetActive(false);
        if (silencerAttachedIcon != null) silencerAttachedIcon.gameObject.SetActive(false);
    }

    private void UpdateSilencerUI()
    {
        if (silencerDurabilitySlider != null)
        {
            silencerDurabilitySlider.maxValue = silencerDurability;
            silencerDurabilitySlider.value = Mathf.Clamp(currentSilencerDurability, 0, silencerDurability);
        }

        DisableAllSilencerIcons();

        int silencerCount = ResourceManager.Instance != null ? ResourceManager.Instance.SilencerCount : 0;
        bool hasAnySilencer = silencerCount > 0;

        if (!hasAnySilencer) return;

        if (isSilencerOn) silencerAttachedIcon?.gameObject.SetActive(true);
        else silencerDetachedIcon?.gameObject.SetActive(true);
    }

    private void ApplySilencerStateVisualAndSound()
    {
        if (isSilencerOn)
        {
            if (silencerModel != null) silencerModel.SetActive(true);
            currentShootSound = silencerSound;
            gunshotLoudness = baseGunshotLoudness * silencedLoudnessMultiplier;
        }
        else
        {
            if (silencerModel != null) silencerModel.SetActive(false);
            currentShootSound = shootSound;
            gunshotLoudness = baseGunshotLoudness;
        }
    }

    private void SaveSilencerRuntimeState()
    {
        if (ResourceManager.Instance == null) return;
        ResourceManager.Instance.SetPistolSilencerRuntimeState(isSilencerOn, currentSilencerDurability);
    }

    public void SetSilencer()
    {
        if (ResourceManager.Instance == null) return;

        // no silencer in inventory => can't attach
        if (ResourceManager.Instance.SilencerCount <= 0) return;

        // if durability somehow zero while still having inventory, prepare fresh unit
        if (currentSilencerDurability <= 0)
            currentSilencerDurability = silencerDurability;

        isSilencerOn = true;

        ApplySilencerStateVisualAndSound();
        UpdateSilencerUI();
        SaveSilencerRuntimeState();
    }

    public void RemoveSilencer()
    {
        isSilencerOn = false;

        ApplySilencerStateVisualAndSound();
        UpdateSilencerUI();
        SaveSilencerRuntimeState();
    }

    protected override void Shoot(RaycastHit hit)
    {
        ApplyDamage(hit, damage);

        bulletOnMag--;
        ResourceManager.Instance?.ConsumePistolAmmo(1);

        nextFireTime = Time.time + fireRate;

        IExplodable explodable = hit.collider.GetComponentInParent<IExplodable>();
        if (explodable != null)
            explodable.Explode();

        NotifyShotEvent();

        if (audioSource && currentShootSound)
            audioSource.PlayOneShot(currentShootSound);

        SoundEmitter.EmitSoundAt(transform.position, SoundType.Gunshot, gunshotLoudness, gameObject);

        if (muzzleFlash)
            StartCoroutine(PlayMuzzleFlash());

        if (ImpactManager.Instance)
            ImpactManager.Instance.SpawnImpact(gunType, hit);

        CinemachineShake.Instance.Shake(recoilIntensity, recoilDuration);

        OnShoot(hit);

        UpdateStatus(bulletOnMag <= 0 ? "Out of Ammo!" : "Aiming");

        if (isSilencerOn)
        {
            currentSilencerDurability--;
            if (silencerDurabilitySlider != null)
                silencerDurabilitySlider.value = currentSilencerDurability;

            if (currentSilencerDurability <= 0)
            {
                currentSilencerDurability = 0;
                isSilencerOn = false;

                // consume only when broken
                ResourceManager.Instance?.ConsumeSilencer(1);

                // if still have spare, prepare next unit durability for MANUAL attach
                if (ResourceManager.Instance != null && ResourceManager.Instance.SilencerCount > 0)
                    currentSilencerDurability = silencerDurability;

                ApplySilencerStateVisualAndSound();
                UpdateSilencerUI();
                SaveSilencerRuntimeState();

                InventoryHandler.Instance?.SyncFromResourceManagerForUI();
            }
            else
            {
                SaveSilencerRuntimeState();
            }
        }

        Debug.Log($"{gameObject.name} fired! Mag: {bulletOnMag}/{magCapacity}, Total: {totalBullet}, Loudness: {gunshotLoudness:F2}");
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isSilencerOn) RemoveSilencer();
            else SetSilencer();
        }
    }
}