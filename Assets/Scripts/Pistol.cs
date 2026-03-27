using System;
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

    

    private bool hasSilencer = true;
    private bool isSilencerOn = false;

    private int currentSilencerDurability;

    private float baseGunshotLoudness;
    private AudioClip currentShootSound;

    protected override void Awake()
    {
        base.Awake();

        

        baseGunshotLoudness = gunshotLoudness;

        silencerDurabilitySlider.maxValue = silencerDurability;
        currentSilencerDurability = silencerDurability;

        UpdateUI(isSilencerOn);
        RemoveSilencer();
    }


    void UpdateUI(bool attached)
    {
        if (silencerAttachedIcon == null || silencerDetachedIcon == null || silencerDurabilitySlider == null)
            return;
        DisableAllSilencerIcons();
        silencerDurabilitySlider.value = currentSilencerDurability;

        if (attached && hasSilencer)
        {
            silencerAttachedIcon.gameObject.SetActive(true);
        }
        else if (!attached && hasSilencer)
        {
            silencerDetachedIcon.gameObject.SetActive(true);
        }
    }

    private void DisableAllSilencerIcons()
    {
       silencerDetachedIcon.gameObject.SetActive(false);
        silencerAttachedIcon.gameObject.SetActive(false);
    }

    public void SetSilencer()
    {
        if (!hasSilencer) return;

        isSilencerOn = true;

        if (silencerModel != null)
            silencerModel.SetActive(true);

        currentShootSound = silencerSound;
        gunshotLoudness = baseGunshotLoudness * silencedLoudnessMultiplier;

        UpdateUI(true);
    }

    public void RemoveSilencer()
    {
        isSilencerOn = false;

        if (silencerModel != null)
            silencerModel.SetActive(false);

        currentShootSound = shootSound;
        gunshotLoudness = baseGunshotLoudness;

        UpdateUI(false);
    }

    protected override void Shoot(RaycastHit hit)
    {
        ApplyDamage(hit, damage);

        bulletOnMag--;

       

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

        Debug.Log($"{gameObject.name} fired! Mag: {bulletOnMag}/{magCapacity}, Total: {totalBullet}, Loudness: {gunshotLoudness:F2}");

        if (isSilencerOn)
        {
            currentSilencerDurability--;
            silencerDurabilitySlider.value = currentSilencerDurability;

            if (currentSilencerDurability <= 0)
            {
                hasSilencer = false;
                RemoveSilencer();
            }
        }

    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!hasSilencer) return;

            if (isSilencerOn)
                RemoveSilencer();
            else
                SetSilencer();
        }
    }

    

}