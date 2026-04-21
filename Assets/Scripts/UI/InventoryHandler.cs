using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryHandler : MonoBehaviour
{
    public static InventoryHandler Instance { get; private set; }
    public static event Action<bool> OnInventoryToggled;

    [Header("Inventory Root")]
    [SerializeField] private Image inventory;

    [Header("Crafting Ingredient Images")]
    [SerializeField] private Image alcohol;
    [SerializeField] private Image rag;
    [SerializeField] private Image binding;
    [SerializeField] private Image gunPowder;
    [SerializeField] private Image canBox;

    [Header("Ingredient Radial Overlay Images")]
    [SerializeField] private Image alcoholRadial;
    [SerializeField] private Image ragRadial;
    [SerializeField] private Image bindingRadial;
    [SerializeField] private Image gunPowderRadial;
    [SerializeField] private Image canRadial;

    [Header("Ingredient Visuals")]
    [SerializeField] private Sprite ingredientNormalSprite;
    [SerializeField] private Sprite ingredientHighlightedSprite;
    [SerializeField] private Sprite ingredientMissingSprite;
    [SerializeField] private Color ingredientNormalColor = Color.white;
    [SerializeField] private Color ingredientHighlightColor = Color.white;
    [SerializeField] private Color ingredientMissingColor = new Color(1f, 0.35f, 0.35f, 1f);

    [Header("Timing")]
    [SerializeField] private float radialDuration = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip notEnoughMaterialsClip;
    [SerializeField] private AudioClip craftingLoopClip;
    [SerializeField, Range(0f, 1f)] private float hoverVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float notEnoughVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float craftingLoopVolume = 1f;

    [Header("Denied Animation - Button")]
    [SerializeField] private float denyAnimDuration = 0.12f;
    [SerializeField] private float denyScaleDown = 0.92f;
    [SerializeField] private float denyScaleUp = 1.03f;

    [Header("Denied Animation - Ingredients Shake")]
    [SerializeField] private float ingredientShakeDuration = 0.16f;
    [SerializeField] private float ingredientShakeFrequency = 45f;
    [SerializeField] private float ingredientShakeAmpNormal = 4f;
    [SerializeField] private float ingredientShakeAmpMissing = 10f;

    private int alcoholCount = 0;
    private int ragCount = 0;
    private int bindingCount = 0;
    private int gunPowderCount = 0;
    private int canCount = 0;

    private int medikitCount = 0;
    private int bandageCount = 0;
    private int silencerCount = 0;
    private int shotgunShellCount = 0;
    private int grenadeCount = 0;
    private int landmineCount = 0;

    [Header("TMP - Ingredient Counts")]
    [SerializeField] private TMP_Text alcoholCountText;
    [SerializeField] private TMP_Text ragCountText;
    [SerializeField] private TMP_Text bindingCountText;
    [SerializeField] private TMP_Text gunPowderCountText;
    [SerializeField] private TMP_Text canCountText;

    [Header("TMP - Craftable Counts")]
    [SerializeField] private TMP_Text medikitCountText;
    [SerializeField] private TMP_Text bandageCountText;
    [SerializeField] private TMP_Text silencerCountText;
    [SerializeField] private TMP_Text shotgunShellCountText;
    [SerializeField] private TMP_Text grenadeCountText;
    [SerializeField] private TMP_Text landmineCountText;

    private enum IngredientType { Alcohol, Rag, Binding, GunPowder, Can }

    private bool isInventoryOpen;
    private Coroutine radialRoutine;
    private Coroutine denyButtonRoutine;
    private readonly Dictionary<Image, Coroutine> ingredientShakeRoutines = new();
    private readonly Dictionary<CustomButton, AudioSource> craftingAudioSources = new();

    private readonly Dictionary<IngredientType, int> ingredientCounts = new();
    private readonly Dictionary<CustomButton.CraftableItem, int> craftableCounts = new();
    private readonly Dictionary<IngredientType, Image> ingredientImages = new();
    private readonly Dictionary<IngredientType, Image> ingredientRadials = new();
    private readonly Dictionary<CustomButton.CraftableItem, Dictionary<IngredientType, int>> recipes = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (inventory != null) inventory.gameObject.SetActive(false);

        ingredientCounts[IngredientType.Alcohol] = alcoholCount;
        ingredientCounts[IngredientType.Rag] = ragCount;
        ingredientCounts[IngredientType.Binding] = bindingCount;
        ingredientCounts[IngredientType.GunPowder] = gunPowderCount;
        ingredientCounts[IngredientType.Can] = canCount;

        craftableCounts[CustomButton.CraftableItem.Medikit] = medikitCount;
        craftableCounts[CustomButton.CraftableItem.Bandage] = bandageCount;
        craftableCounts[CustomButton.CraftableItem.Silencer] = silencerCount;
        craftableCounts[CustomButton.CraftableItem.ShotgunShell] = shotgunShellCount;
        craftableCounts[CustomButton.CraftableItem.Grenade] = grenadeCount;
        craftableCounts[CustomButton.CraftableItem.Landmine] = landmineCount;

        ingredientImages[IngredientType.Alcohol] = alcohol;
        ingredientImages[IngredientType.Rag] = rag;
        ingredientImages[IngredientType.Binding] = binding;
        ingredientImages[IngredientType.GunPowder] = gunPowder;
        ingredientImages[IngredientType.Can] = canBox;

        ingredientRadials[IngredientType.Alcohol] = alcoholRadial;
        ingredientRadials[IngredientType.Rag] = ragRadial;
        ingredientRadials[IngredientType.Binding] = bindingRadial;
        ingredientRadials[IngredientType.GunPowder] = gunPowderRadial;
        ingredientRadials[IngredientType.Can] = canRadial;

        BuildRecipes();
        SetupRadials();
        ResetIngredientVisuals();

        // force UI to use manager values after scene reload
        SyncFromResourceManagerForUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isInventoryOpen) CloseInventory();
            else OpenInventory();
        }
    }

    private void OnEnable()
    {
        CustomButton.OnHoveredCraftItem += HandleHoveredCraftItem;
        CustomButton.OnUnhoveredCraftItem += HandleUnhoveredCraftItem;
        CustomButton.OnCraftItemHold += HandleCraftItemHold;
        CustomButton.OnCraftItemRelease += HandleCraftItemRelease;
        CustomButton.OnCraftCompleted += HandleCraftCompleted;
        CustomButton.OnCraftDenied += HandleCraftDenied;
        CustomButton.OnCraftCanceled += HandleCraftCanceled;

        ResourceManager.OnBandageFullChanged += OnManagerCountsChanged;
        ResourceManager.OnGrenadeFullChanged += OnManagerCountsChanged;
        ResourceManager.OnLandmineFullChanged += OnManagerCountsChanged;
        ResourceManager.OnShotgunShellFullChanged += OnManagerCountsChanged;
        ResourceManager.OnMedkitFullChanged += OnManagerCountsChanged;
    }

    private void OnDisable()
    {
        CustomButton.OnHoveredCraftItem -= HandleHoveredCraftItem;
        CustomButton.OnUnhoveredCraftItem -= HandleUnhoveredCraftItem;
        CustomButton.OnCraftItemHold -= HandleCraftItemHold;
        CustomButton.OnCraftItemRelease -= HandleCraftItemRelease;
        CustomButton.OnCraftCompleted -= HandleCraftCompleted;
        CustomButton.OnCraftDenied -= HandleCraftDenied;
        CustomButton.OnCraftCanceled -= HandleCraftCanceled;

        ResourceManager.OnBandageFullChanged -= OnManagerCountsChanged;
        ResourceManager.OnGrenadeFullChanged -= OnManagerCountsChanged;
        ResourceManager.OnLandmineFullChanged -= OnManagerCountsChanged;
        ResourceManager.OnShotgunShellFullChanged -= OnManagerCountsChanged;
        ResourceManager.OnMedkitFullChanged -= OnManagerCountsChanged;

        StopAllCraftingLoopSounds();
        StopRadialRoutine();
        HideAllRadials();
    }

    private void OnManagerCountsChanged(bool _)
    {
        SyncFromResourceManagerForUI();
    }

    // public add methods for ResourceManager
    public void AddAlcohol(int amount)
    {
        alcoholCount = Mathf.Max(0, alcoholCount + amount);
        ingredientCounts[IngredientType.Alcohol] = alcoholCount;
        UpdateUICounts();
    }

    public void AddRag(int amount)
    {
        ragCount = Mathf.Max(0, ragCount + amount);
        ingredientCounts[IngredientType.Rag] = ragCount;
        UpdateUICounts();
    }

    public void AddBinding(int amount)
    {
        bindingCount = Mathf.Max(0, bindingCount + amount);
        ingredientCounts[IngredientType.Binding] = bindingCount;
        UpdateUICounts();
    }

    public void AddGunPowder(int amount)
    {
        gunPowderCount = Mathf.Max(0, gunPowderCount + amount);
        ingredientCounts[IngredientType.GunPowder] = gunPowderCount;
        UpdateUICounts();
    }

    public void AddCan(int amount)
    {
        canCount = Mathf.Max(0, canCount + amount);
        ingredientCounts[IngredientType.Can] = canCount;
        UpdateUICounts();
    }

    public void AddMedkit(int amount)
    {
        medikitCount = Mathf.Max(0, medikitCount + amount);
        craftableCounts[CustomButton.CraftableItem.Medikit] = medikitCount;
        UpdateUICounts();
    }

    public void AddBandageCraftable(int amount)
    {
        bandageCount = Mathf.Max(0, bandageCount + amount);
        craftableCounts[CustomButton.CraftableItem.Bandage] = bandageCount;
        UpdateUICounts();
    }

    public void AddSilencer(int amount)
    {
        silencerCount = Mathf.Max(0, silencerCount + amount);
        craftableCounts[CustomButton.CraftableItem.Silencer] = silencerCount;
        UpdateUICounts();
    }

    public bool CanCraft(CustomButton.CraftableItem item)
    {
        if (!recipes.TryGetValue(item, out var recipe)) return false;
        foreach (var req in recipe)
            if (ingredientCounts[req.Key] < req.Value) return false;
        return true;
    }

    public void UpdateUICounts()
    {
        if (alcoholCountText) alcoholCountText.text = ingredientCounts[IngredientType.Alcohol].ToString();
        if (ragCountText) ragCountText.text = ingredientCounts[IngredientType.Rag].ToString();
        if (bindingCountText) bindingCountText.text = ingredientCounts[IngredientType.Binding].ToString();
        if (gunPowderCountText) gunPowderCountText.text = ingredientCounts[IngredientType.GunPowder].ToString();
        if (canCountText) canCountText.text = ingredientCounts[IngredientType.Can].ToString();

        if (medikitCountText) medikitCountText.text = craftableCounts[CustomButton.CraftableItem.Medikit].ToString();
        if (bandageCountText) bandageCountText.text = craftableCounts[CustomButton.CraftableItem.Bandage].ToString();
        if (silencerCountText) silencerCountText.text = craftableCounts[CustomButton.CraftableItem.Silencer].ToString();
        if (shotgunShellCountText) shotgunShellCountText.text = craftableCounts[CustomButton.CraftableItem.ShotgunShell].ToString();
        if (grenadeCountText) grenadeCountText.text = craftableCounts[CustomButton.CraftableItem.Grenade].ToString();
        if (landmineCountText) landmineCountText.text = craftableCounts[CustomButton.CraftableItem.Landmine].ToString();
    }

    private void HandleHoveredCraftItem(CustomButton button)
    {
        PlayHoverSfx();
        HighlightRecipe(button.ItemToCraft);
    }

    private void HandleUnhoveredCraftItem(CustomButton button)
    {
        StopRadialRoutine();
        HideAllRadials();
        ResetIngredientVisuals();
    }

    private void HandleCraftItemHold(CustomButton button)
    {
        var item = button.ItemToCraft;
        HighlightRecipe(item);

        if (!CanCraft(item))
        {
            PlayNotEnoughSfx();
            PlayDeniedButtonBump(button);
            PlayIngredientDenyShake(item);
            StopRadialRoutine();
            HideAllRadials();
            StopCraftingLoopSound(button);
            return;
        }

        StartCraftingLoopSound(button);
        StopRadialRoutine();
        radialRoutine = StartCoroutine(IngredientRadialFillRoutine(item, radialDuration));
    }

    private void HandleCraftItemRelease(CustomButton button)
    {
        StopCraftingLoopSound(button);
        StopRadialRoutine();
        HideAllRadials();
        HighlightRecipe(button.ItemToCraft);
    }

    private void HandleCraftCanceled(CustomButton button)
    {
        StopCraftingLoopSound(button);
        StopRadialRoutine();
        HideAllRadials();
    }

    private void HandleCraftCompleted(CustomButton button)
    {
        StopCraftingLoopSound(button);
        StopRadialRoutine();
        HideAllRadials();

        if (!TryCraft(button.ItemToCraft))
        {
            PlayNotEnoughSfx();
            PlayDeniedButtonBump(button);
            PlayIngredientDenyShake(button.ItemToCraft);
            return;
        }

        UpdateUICounts();
        HighlightRecipe(button.ItemToCraft);
    }

    private void HandleCraftDenied(CustomButton button)
    {
        StopCraftingLoopSound(button);
        PlayNotEnoughSfx();
        PlayDeniedButtonBump(button);
        PlayIngredientDenyShake(button.ItemToCraft);
        StopRadialRoutine();
        HideAllRadials();
        HighlightRecipe(button.ItemToCraft);
    }

    // consume ingredients via ResourceManager (as requested)
    private bool TryCraft(CustomButton.CraftableItem item)
    {
        if (!CanCraft(item)) return false;
        if (!recipes.TryGetValue(item, out var recipe)) return false;
        if (ResourceManager.Instance == null) return false;

        foreach (var req in recipe)
        {
            bool ok = req.Key switch
            {
                IngredientType.Alcohol => ResourceManager.Instance.ConsumeAlcohol(req.Value),
                IngredientType.Rag => ResourceManager.Instance.ConsumeRag(req.Value),
                IngredientType.Binding => ResourceManager.Instance.ConsumeBinding(req.Value),
                IngredientType.GunPowder => ResourceManager.Instance.ConsumeGunpowder(req.Value),
                IngredientType.Can => ResourceManager.Instance.ConsumeCan(req.Value),
                _ => false
            };

            if (!ok) return false;
            ingredientCounts[req.Key] -= req.Value;
        }

        alcoholCount = ingredientCounts[IngredientType.Alcohol];
        ragCount = ingredientCounts[IngredientType.Rag];
        bindingCount = ingredientCounts[IngredientType.Binding];
        gunPowderCount = ingredientCounts[IngredientType.GunPowder];
        canCount = ingredientCounts[IngredientType.Can];

        switch (item)
        {
            case CustomButton.CraftableItem.Bandage:
                ResourceManager.Instance.SetBandage(1);
                break;
            case CustomButton.CraftableItem.Grenade:
                ResourceManager.Instance.SetGrenade(1);
                break;
            case CustomButton.CraftableItem.Landmine:
                ResourceManager.Instance.SetLandmine(1);
                break;
            case CustomButton.CraftableItem.ShotgunShell:
                ResourceManager.Instance.SetShotgunShell(4);
                break;
            case CustomButton.CraftableItem.Medikit:
                ResourceManager.Instance.SetMedkit(1); 
                var sw = FindFirstObjectByType<SwitchWeapons>(FindObjectsInactive.Include);
                sw?.SyncFromResourceManager();
                break;
            case CustomButton.CraftableItem.Silencer:
                ResourceManager.Instance.SetSilencer(1);
                break;
        }

        SyncFromResourceManagerForUI();
        return true;
    }

    public void SyncFromResourceManagerForUI()
    {
        if (ResourceManager.Instance == null) return;

        // Ingredients
        alcoholCount = ResourceManager.Instance.AlcoholCount;
        ragCount = ResourceManager.Instance.RagCount;
        bindingCount = ResourceManager.Instance.BindingCount;
        gunPowderCount = ResourceManager.Instance.GunpowderCount;
        canCount = ResourceManager.Instance.CanCount;
        silencerCount = ResourceManager.Instance.SilencerCount;

        ingredientCounts[IngredientType.Alcohol] = alcoholCount;
        ingredientCounts[IngredientType.Rag] = ragCount;
        ingredientCounts[IngredientType.Binding] = bindingCount;
        ingredientCounts[IngredientType.GunPowder] = gunPowderCount;
        ingredientCounts[IngredientType.Can] = canCount;

        // Craftables
        bandageCount = ResourceManager.Instance.BandageCount;
        grenadeCount = ResourceManager.Instance.GrenadeCount;
        landmineCount = ResourceManager.Instance.LandmineCount;
        medikitCount = ResourceManager.Instance.MedkitCount;
        shotgunShellCount = ResourceManager.Instance.ShotgunShellCount;

        craftableCounts[CustomButton.CraftableItem.Bandage] = bandageCount;
        craftableCounts[CustomButton.CraftableItem.Grenade] = grenadeCount;
        craftableCounts[CustomButton.CraftableItem.Landmine] = landmineCount;
        craftableCounts[CustomButton.CraftableItem.Medikit] = medikitCount;
        craftableCounts[CustomButton.CraftableItem.ShotgunShell] = shotgunShellCount;
        craftableCounts[CustomButton.CraftableItem.Silencer] = silencerCount;

        UpdateUICounts();
    }

    private IEnumerator IngredientRadialFillRoutine(CustomButton.CraftableItem item, float duration)
    {
        if (!recipes.TryGetValue(item, out var recipe)) yield break;

        foreach (var req in recipe)
        {
            Image radial = ingredientRadials[req.Key];
            if (radial == null) continue;
            radial.gameObject.SetActive(true);
            radial.fillAmount = 0f;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);

            foreach (var req in recipe)
            {
                Image radial = ingredientRadials[req.Key];
                if (radial == null) continue;
                radial.fillAmount = p;
            }

            yield return null;
        }

        radialRoutine = null;
    }

    private void HighlightRecipe(CustomButton.CraftableItem item)
    {
        ResetIngredientVisuals();
        if (!recipes.TryGetValue(item, out var recipe)) return;

        foreach (var req in recipe)
        {
            IngredientType ing = req.Key;
            int need = req.Value;
            int have = ingredientCounts[ing];

            Image img = ingredientImages[ing];
            if (img == null) continue;

            bool enough = have >= need;
            if (enough)
            {
                if (ingredientHighlightedSprite != null) img.sprite = ingredientHighlightedSprite;
                img.color = ingredientHighlightColor;
            }
            else
            {
                if (ingredientMissingSprite != null) img.sprite = ingredientMissingSprite;
                img.color = ingredientMissingColor;
            }
        }
    }

    private void ResetIngredientVisuals()
    {
        foreach (var kvp in ingredientImages)
        {
            if (kvp.Value == null) continue;
            if (ingredientNormalSprite != null) kvp.Value.sprite = ingredientNormalSprite;
            kvp.Value.color = ingredientNormalColor;
        }
    }

    private void SetupRadials()
    {
        foreach (var kvp in ingredientRadials)
        {
            if (kvp.Value == null) continue;
            kvp.Value.type = Image.Type.Filled;
            kvp.Value.fillMethod = Image.FillMethod.Radial360;
            kvp.Value.fillOrigin = (int)Image.Origin360.Top;
            kvp.Value.fillClockwise = true;
            kvp.Value.fillAmount = 0f;
            kvp.Value.gameObject.SetActive(false);
        }
    }

    private void HideAllRadials()
    {
        foreach (var kvp in ingredientRadials)
        {
            if (kvp.Value == null) continue;
            kvp.Value.fillAmount = 0f;
            kvp.Value.gameObject.SetActive(false);
        }
    }

    private void StopRadialRoutine()
    {
        if (radialRoutine != null)
        {
            StopCoroutine(radialRoutine);
            radialRoutine = null;
        }
    }

    private void PlayHoverSfx()
    {
        if (audioSource != null && hoverClip != null)
            audioSource.PlayOneShot(hoverClip, hoverVolume);
    }

    private void PlayNotEnoughSfx()
    {
        if (audioSource != null && notEnoughMaterialsClip != null)
            audioSource.PlayOneShot(notEnoughMaterialsClip, notEnoughVolume);
    }

    private void StartCraftingLoopSound(CustomButton button)
    {
        if (button == null || craftingLoopClip == null) return;

        if (!craftingAudioSources.TryGetValue(button, out var src) || src == null)
        {
            src = button.gameObject.GetComponent<AudioSource>();
            if (src == null) src = button.gameObject.AddComponent<AudioSource>();
            craftingAudioSources[button] = src;
        }

        src.clip = craftingLoopClip;
        src.loop = true;
        src.playOnAwake = false;
        src.volume = craftingLoopVolume;

        if (!src.isPlaying) src.Play();
    }

    private void StopCraftingLoopSound(CustomButton button)
    {
        if (button == null) return;
        if (craftingAudioSources.TryGetValue(button, out var src) && src != null && src.isPlaying) src.Stop();
    }

    private void StopAllCraftingLoopSounds()
    {
        foreach (var kvp in craftingAudioSources)
        {
            if (kvp.Value != null && kvp.Value.isPlaying)
                kvp.Value.Stop();
        }
    }

    private void PlayDeniedButtonBump(CustomButton button)
    {
        if (button == null || button.TargetImage == null) return;

        RectTransform rt = button.TargetImage.rectTransform;
        if (denyButtonRoutine != null) StopCoroutine(denyButtonRoutine);
        denyButtonRoutine = StartCoroutine(DenyButtonBumpRoutine(rt));
    }

    private IEnumerator DenyButtonBumpRoutine(RectTransform rt)
    {
        if (rt == null) yield break;

        Vector3 baseScale = Vector3.one;
        float half = denyAnimDuration * 0.5f;
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / half);
            float s = Mathf.Lerp(1f, denyScaleDown, p);
            rt.localScale = new Vector3(s, s, s);
            yield return null;
        }

        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / half);
            float s = Mathf.Lerp(denyScaleDown, denyScaleUp, p);
            rt.localScale = new Vector3(s, s, s);
            yield return null;
        }

        rt.localScale = baseScale;
        denyButtonRoutine = null;
    }

    private void PlayIngredientDenyShake(CustomButton.CraftableItem item)
    {
        if (!recipes.TryGetValue(item, out var recipe)) return;

        foreach (var req in recipe)
        {
            if (!ingredientImages.TryGetValue(req.Key, out var img) || img == null) continue;

            int have = ingredientCounts[req.Key];
            bool missing = have < req.Value;
            float amp = missing ? ingredientShakeAmpMissing : ingredientShakeAmpNormal;

            StartIngredientShake(img, amp, ingredientShakeDuration);
        }
    }

    private void StartIngredientShake(Image img, float amplitude, float duration)
    {
        if (ingredientShakeRoutines.TryGetValue(img, out var running) && running != null)
            StopCoroutine(running);

        ingredientShakeRoutines[img] = StartCoroutine(IngredientShakeRoutine(img.rectTransform, amplitude, duration, img));
    }

    private IEnumerator IngredientShakeRoutine(RectTransform rt, float amp, float duration, Image key)
    {
        if (rt == null) yield break;

        Vector2 basePos = rt.anchoredPosition;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float damper = 1f - (t / duration);
            float x = Mathf.Sin(t * ingredientShakeFrequency) * amp * damper;
            rt.anchoredPosition = basePos + new Vector2(x, 0f);
            yield return null;
        }

        rt.anchoredPosition = basePos;
        ingredientShakeRoutines[key] = null;
    }

    private void BuildRecipes()
    {
        recipes[CustomButton.CraftableItem.Medikit] = new() {
            { IngredientType.Alcohol, 1 }, { IngredientType.Rag, 2 }, { IngredientType.Binding, 1 }
        };
        recipes[CustomButton.CraftableItem.Bandage] = new() {
            { IngredientType.Rag, 1 }, { IngredientType.Binding, 1 }
        };
        recipes[CustomButton.CraftableItem.Silencer] = new() {
            { IngredientType.Can, 1 }, { IngredientType.Rag, 1 }, { IngredientType.Binding, 1 }
        };
        recipes[CustomButton.CraftableItem.ShotgunShell] = new() {
            { IngredientType.Can, 1 }, { IngredientType.GunPowder, 1 }
        };
        recipes[CustomButton.CraftableItem.Grenade] = new() {
            { IngredientType.Can, 1 }, { IngredientType.GunPowder, 2 }, { IngredientType.Binding, 1 }
        };
        recipes[CustomButton.CraftableItem.Landmine] = new() {
            { IngredientType.Can, 1 }, { IngredientType.GunPowder, 2 }, { IngredientType.Binding, 1 }
        };
    }

    private void OpenInventory()
    {
        if (inventory != null) inventory.gameObject.SetActive(true);
        isInventoryOpen = true;
        OnInventoryToggled?.Invoke(true);
    }

    private void CloseInventory()
    {
        if (inventory != null) inventory.gameObject.SetActive(false);
        isInventoryOpen = false;
        OnInventoryToggled?.Invoke(false);
    }
}