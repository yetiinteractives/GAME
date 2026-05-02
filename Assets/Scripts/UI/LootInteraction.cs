using UnityEngine;

public class LootInteraction : MonoBehaviour, IInteractable
{
    public InteractablesEnum interactables;
    public LevelFlag levelFlag = LevelFlag.None;

    public int amount = 10;

    [Header("UI")]
    [SerializeField] private GameObject lootIcon;
    [SerializeField] private GameObject promptText;

    [Header("Sprite Tint")]
    [SerializeField] private SpriteRenderer lootIconSprite;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color fullColor = new Color(1f, 0.45f, 0.45f, 1f);

    private bool isFullForThisLoot;
    private bool isRemoved;
    private PersistentSceneEntity persistentEntity;

    private void Awake()
    {
        persistentEntity = GetComponent<PersistentSceneEntity>();
    }

    private void Start()
    {
        if (lootIconSprite == null && lootIcon != null)
            lootIconSprite = lootIcon.GetComponent<SpriteRenderer>();

        HideInteractableIcon();
        HideInteractionPrompt();
        RefreshVisualState();
    }

    private void OnEnable()
    {
        SubscribeAll();
        RefreshVisualState();
    }

    private void OnDisable()
    {
        UnsubscribeAll();
    }

    public void ShowInteractableIcon()
    {
        if (isRemoved) return;
        if (lootIcon != null) lootIcon.SetActive(true);
        RefreshVisualState();
    }

    public void HideInteractableIcon()
    {
        if (isRemoved) return;
        if (lootIcon != null) lootIcon.SetActive(false);
    }

    public void ShowInteractionPrompt()
    {
        if (isRemoved) return;
        if (isFullForThisLoot)
        {
            HideInteractionPrompt();
            return;
        }
        if (promptText != null) promptText.SetActive(true);
    }

    public void HideInteractionPrompt()
    {
        if (isRemoved) return;
        if (promptText != null) promptText.SetActive(false);
    }

    public void Interact()
    {
        if (isFullForThisLoot || isRemoved) return;

        bool success = TryInteract();
        if (!success) return;

        isRemoved = true;
        enabled = false;
        HideInteractableIcon();
        HideInteractionPrompt();

        if (persistentEntity != null)
            persistentEntity.MarkRemoved();
        else
            Destroy(gameObject);
    }

    private bool TryInteract()
    {
        if (ResourceManager.Instance == null) return false;

        switch (interactables)
        {
            case InteractablesEnum.pistolAmmo:
                ResourceManager.Instance.AddPistolReserve(amount);
                break;

            case InteractablesEnum.shotgunAmmo:
                ResourceManager.Instance.AddShotgunReserve(amount);
                break;

            case InteractablesEnum.sniperAmmo:
                ResourceManager.Instance.AddSniperReserve(amount);
                break;

            case InteractablesEnum.grenade:
                ResourceManager.Instance.SetGrenadeAbsolute(ResourceManager.Instance.GrenadeCount + amount);
                break;

            case InteractablesEnum.landmine:
                ResourceManager.Instance.SetLandmineAbsolute(ResourceManager.Instance.LandmineCount + amount);
                break;

            case InteractablesEnum.bandage:
                ResourceManager.Instance.SetBandageAbsolute(ResourceManager.Instance.BandageCount + amount);
                break;

            case InteractablesEnum.medkit:
                ResourceManager.Instance.SetMedkitAbsolute(ResourceManager.Instance.MedkitCount + amount);
                break;

            case InteractablesEnum.alchohol:
                ResourceManager.Instance.SetAlcoholAbsolute(ResourceManager.Instance.AlcoholCount + amount);
                break;

            case InteractablesEnum.rag:
                ResourceManager.Instance.SetRagAbsolute(ResourceManager.Instance.RagCount + amount);
                break;

            case InteractablesEnum.binding:
                ResourceManager.Instance.SetBindingAbsolute(ResourceManager.Instance.BindingCount + amount);
                break;

            case InteractablesEnum.gunpowder:
                ResourceManager.Instance.SetGunpowderAbsolute(ResourceManager.Instance.GunpowderCount + amount);
                break;

            case InteractablesEnum.can:
                ResourceManager.Instance.SetCanAbsolute(ResourceManager.Instance.CanCount + amount);
                break;
            case InteractablesEnum.key:
                PickUpKey(levelFlag);
                break;

            default:
                return false;
        }
        ResourceManager.Instance.CaptureRuntimeAmmoFromWeapons();
        ResourceManager.Instance.ForceResyncAllRuntimeUsers();
        ResourceManager.Instance.BroadcastAllFullStatesPublic();
        InventoryHandler.Instance?.SyncFromResourceManagerForUI();

        return true;
    }

    private void ApplyFullState(bool isFull)
    {
        isFullForThisLoot = isFull;
        RefreshVisualState();
        if (isFullForThisLoot) HideInteractionPrompt();
    }

    private void RefreshVisualState()
    {
        if (lootIconSprite != null)
            lootIconSprite.color = isFullForThisLoot ? fullColor : normalColor;
    }

    void PickUpKey(LevelFlag key)
    {
        LevelManager.Instance.TriggerFlag(key);
    }

    private void SubscribeAll()
    {
        ResourceManager.OnPistolAmmoFullChanged += OnPistolAmmoFullChanged;
        ResourceManager.OnShotgunAmmoFullChanged += OnShotgunAmmoFullChanged;
        ResourceManager.OnSniperAmmoFullChanged += OnSniperAmmoFullChanged;
        ResourceManager.OnGrenadeFullChanged += OnGrenadeFullChanged;
        ResourceManager.OnLandmineFullChanged += OnLandmineFullChanged;
        ResourceManager.OnMedkitFullChanged += OnMedkitFullChanged;
        ResourceManager.OnBandageFullChanged += OnBandageFullChanged;
        ResourceManager.OnSilencerFullChanged += OnSilencerFullChanged;
        ResourceManager.OnAlcoholFullChanged += OnAlcoholFullChanged;
        ResourceManager.OnRagFullChanged += OnRagFullChanged;
        ResourceManager.OnBindingFullChanged += OnBindingFullChanged;
        ResourceManager.OnGunpowderFullChanged += OnGunpowderFullChanged;
        ResourceManager.OnCanFullChanged += OnCanFullChanged;
    }

    private void UnsubscribeAll()
    {
        ResourceManager.OnPistolAmmoFullChanged -= OnPistolAmmoFullChanged;
        ResourceManager.OnShotgunAmmoFullChanged -= OnShotgunAmmoFullChanged;
        ResourceManager.OnSniperAmmoFullChanged -= OnSniperAmmoFullChanged;
        ResourceManager.OnGrenadeFullChanged -= OnGrenadeFullChanged;
        ResourceManager.OnLandmineFullChanged -= OnLandmineFullChanged;
        ResourceManager.OnMedkitFullChanged -= OnMedkitFullChanged;
        ResourceManager.OnBandageFullChanged -= OnBandageFullChanged;
        ResourceManager.OnSilencerFullChanged -= OnSilencerFullChanged;
        ResourceManager.OnAlcoholFullChanged -= OnAlcoholFullChanged;
        ResourceManager.OnRagFullChanged -= OnRagFullChanged;
        ResourceManager.OnBindingFullChanged -= OnBindingFullChanged;
        ResourceManager.OnGunpowderFullChanged -= OnGunpowderFullChanged;
        ResourceManager.OnCanFullChanged -= OnCanFullChanged;
    }

    private void OnPistolAmmoFullChanged(bool v) { if (interactables == InteractablesEnum.pistolAmmo) ApplyFullState(v); }
    private void OnShotgunAmmoFullChanged(bool v) { if (interactables == InteractablesEnum.shotgunAmmo) ApplyFullState(v); }
    private void OnSniperAmmoFullChanged(bool v) { if (interactables == InteractablesEnum.sniperAmmo) ApplyFullState(v); }
    private void OnGrenadeFullChanged(bool v) { if (interactables == InteractablesEnum.grenade) ApplyFullState(v); }
    private void OnLandmineFullChanged(bool v) { if (interactables == InteractablesEnum.landmine) ApplyFullState(v); }
    private void OnMedkitFullChanged(bool v) { if (interactables == InteractablesEnum.medkit) ApplyFullState(v); }
    private void OnBandageFullChanged(bool v) { if (interactables == InteractablesEnum.bandage) ApplyFullState(v); }
    private void OnSilencerFullChanged(bool v) { if (interactables == InteractablesEnum.silencer) ApplyFullState(v); }
    private void OnAlcoholFullChanged(bool v) { if (interactables == InteractablesEnum.alchohol) ApplyFullState(v); }
    private void OnRagFullChanged(bool v) { if (interactables == InteractablesEnum.rag) ApplyFullState(v); }
    private void OnBindingFullChanged(bool v) { if (interactables == InteractablesEnum.binding) ApplyFullState(v); }
    private void OnGunpowderFullChanged(bool v) { if (interactables == InteractablesEnum.gunpowder) ApplyFullState(v); }
    private void OnCanFullChanged(bool v) { if (interactables == InteractablesEnum.can) ApplyFullState(v); }
}