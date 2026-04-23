using UnityEngine;

public class LootInteraction : MonoBehaviour, IInteractable
{
    public InteractablesEnum interactables;
    public int amount = 10;

    [Header("UI")]
    [SerializeField] private GameObject lootIcon;
    [SerializeField] private GameObject promptText;

    [Header("Sprite Tint")]
    [SerializeField] private SpriteRenderer lootIconSprite;   // <-- sprite renderer
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color fullColor = new Color(1f, 0.45f, 0.45f, 1f);

    private bool isFullForThisLoot;

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
        lootIcon?.SetActive(true);
        RefreshVisualState();
    }

    public void HideInteractableIcon() => lootIcon?.SetActive(false);

    public void ShowInteractionPrompt()
    {
        if (isFullForThisLoot)
        {
            HideInteractionPrompt();
            return;
        }
        promptText?.SetActive(true);
    }

    public void HideInteractionPrompt()
    {
        if (promptText != null) promptText.SetActive(false);
    }

    public void Interact()
    {
        if (isFullForThisLoot) return;
        TryInteract();
        gameObject.SetActive(false);
    }

    public void TryInteract()
    {
        if (ResourceManager.Instance == null) return;

        switch (interactables)
        {
            case InteractablesEnum.pistolAmmo:
                ResourceManager.Instance.SetPistolReserveAbsolute(ResourceManager.Instance.PistolAmmoCount + amount);
                break;

            case InteractablesEnum.shotgunAmmo:
                ResourceManager.Instance.SetShotgunReserveAbsolute(ResourceManager.Instance.ShotgunAmmoCount + amount);
                break;

            case InteractablesEnum.sniperAmmo:
                ResourceManager.Instance.SetSniperReserveAbsolute(ResourceManager.Instance.SniperAmmoCount + amount);
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
        }

        ResourceManager.Instance.ForceResyncAllRuntimeUsers();
        ResourceManager.Instance.BroadcastAllFullStatesPublic(); // add public wrapper if needed
        InventoryHandler.Instance?.SyncFromResourceManagerForUI();
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

    // subscribe all
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

    // unsubscribe all
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

    // route by enum
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