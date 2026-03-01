using UnityEngine;
using UnityEngine.UI;

public class Pistol : Weapon
{
    [Header("Silencer")]
    [SerializeField] private int silencerDurability = 5;

    [SerializeField] private GameObject silencerModel;
    [SerializeField] private AudioClip silencerSound;

    [Header("UI")]
    [SerializeField] private Image silencerIcon;
    private Vector2 originalIconPosition;
    private Vector2 detachedOffset = new Vector2(10, 0);

    [SerializeField] private Color silencerAttachedColor = Color.white;
    [SerializeField] private Color silencerDetachedColor = Color.gray;

    [Header("Sound")]
    [SerializeField, Range(0.01f, 1f)]
    private float silencedLoudnessMultiplier = 0.1f;

    private RectTransform iconRect;

    private bool hasSilencer = true;
    private bool isSilencerOn;

    private int currentSilencerDurability;

    private float baseGunshotLoudness;
    private AudioClip currentShootSound;

    protected override void Awake()
    {
        base.Awake();

        iconRect = silencerIcon.rectTransform;

        baseGunshotLoudness = gunshotLoudness;
        currentSilencerDurability = silencerDurability;

        UpdateUI(false);
        RemoveSilencer();
    }
    private void Start()
    {
        originalIconPosition = silencerIcon.rectTransform.localPosition;
    }

    void UpdateUI(bool attached)
    {
        if (silencerIcon == null) return;

        silencerIcon.gameObject.SetActive(hasSilencer);

        if (attached)
        {
            silencerIcon.color = silencerAttachedColor;
            //iconRect.localPosition = originalIconPosition;
        }
        else
        {
            silencerIcon.color = silencerDetachedColor;
            //iconRect.localPosition = originalIconPosition + detachedOffset;
        }
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