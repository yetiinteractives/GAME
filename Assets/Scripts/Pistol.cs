using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class Pistol : Weapon
{
    [SerializeField] private GameObject silencerModel;
    [SerializeField] private AudioClip silencerSound;
    [SerializeField] private Image silencerIcon;

    [SerializeField] private int silencerDurability = 5;
    
    private int currentSilencerDurability;

    [Header("Silencer")]
    [SerializeField, Range(0.01f, 1f)] private float silencedLoudnessMultiplier = 0.1f;

    private AudioClip currentShootSound;
    private bool isSilencerOn;
    private float baseGunshotLoudness;

    protected override void Awake()
    {
        base.Awake();

        baseGunshotLoudness = gunshotLoudness;
        RemoveSilencer();
        currentSilencerDurability = silencerDurability;
    }

    public void SetSilencer()
    {
        isSilencerOn = true;

        if (silencerModel != null)
            silencerModel.SetActive(true);

        silencerIcon.gameObject.SetActive(true);
        currentShootSound = silencerSound;
        gunshotLoudness = baseGunshotLoudness * silencedLoudnessMultiplier;
    }

    public void RemoveSilencer()
    {
        isSilencerOn = false;

        if (silencerModel != null)
            silencerModel.SetActive(false);

        silencerIcon.gameObject.SetActive(false);
        currentShootSound = shootSound;
        gunshotLoudness = baseGunshotLoudness;
    }

    protected override void Shoot(RaycastHit hit)
    {
        ApplyDamage(hit, damage);

        bulletOnMag--;
        if (isSilencerOn)
        {
            currentSilencerDurability--;
        }
        nextFireTime = Time.time + fireRate;

        NotifyShotEvent();

        if (audioSource != null && currentShootSound != null)
            audioSource.PlayOneShot(currentShootSound);

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

        Debug.Log($"{gameObject.name} fired! Mag: {bulletOnMag}/{magCapacity}, Total: {totalBullet}, Loudness: {gunshotLoudness:F2}");
    
        if(currentSilencerDurability <= 0)
        {
            RemoveSilencer();
        }
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isSilencerOn)
                RemoveSilencer();
            else
                SetSilencer();
        }
    }
}