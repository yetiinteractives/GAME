using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryHandler : MonoBehaviour
{
    public static event Action<bool> OnInventoryToggled;

    [SerializeField] private Image inventory;

    [Header("Craftble Items Images")]
    [SerializeField] private Image medikit;
    [SerializeField] private Image bandage;
    [SerializeField] private Image silencer;
    [SerializeField] private Image shotgunShell;
    [SerializeField] private Image grenade;
    [SerializeField] private Image landmine;

    [Header("Crafting Items Images")]
    [SerializeField] private Image alcohol;
    [SerializeField] private Image rag;
    [SerializeField] private Image binding;
    [SerializeField] private Image gunPowder;
    [SerializeField] private Image canBox;

    [Header("Crafting Item Highlight")]
    [SerializeField] private Sprite ingredientNormalSprite;
    [SerializeField] private Sprite ingredientHighlightedSprite;
    [SerializeField] private Sprite ingredientMissingSprite;
    [SerializeField] private Color ingredientNormalColor = Color.white;
    [SerializeField] private Color ingredientHighlightColor = Color.white;
    [SerializeField] private Color ingredientMissingColor = new Color(1f, 0.35f, 0.35f, 1f);

    [Header("Radial Fill Overlay (for crafting ingredients)")]
    [SerializeField] private Image alcoholRadial;
    [SerializeField] private Image ragRadial;
    [SerializeField] private Image bindingRadial;
    [SerializeField] private Image gunPowderRadial;
    [SerializeField] private Image canRadial;
    [SerializeField] private float radialDuration = 1.5f;

    [Header("Initial Crafting Item Counts")]
    [SerializeField] private int alcoholCount = 3;
    [SerializeField] private int ragCount = 5;
    [SerializeField] private int bindingCount = 3;
    [SerializeField] private int gunPowderCount = 4;
    [SerializeField] private int canCount = 3;

    [Header("Initial Craftable Item Counts")]
    [SerializeField] private int medikitCount = 0;
    [SerializeField] private int bandageCount = 0;
    [SerializeField] private int silencerCount = 0;
    [SerializeField] private int shotgunShellCount = 0;
    [SerializeField] private int grenadeCount = 0;
    [SerializeField] private int landmineCount = 0;

    [Header("TMP - Crafting Item Counts")]
    [SerializeField] private TMP_Text alcoholCountText;
    [SerializeField] private TMP_Text ragCountText;
    [SerializeField] private TMP_Text bindingCountText;
    [SerializeField] private TMP_Text gunPowderCountText;
    [SerializeField] private TMP_Text canCountText;

    [Header("TMP - Craftable Item Counts")]
    [SerializeField] private TMP_Text medikitCountText;
    [SerializeField] private TMP_Text bandageCountText;
    [SerializeField] private TMP_Text silencerCountText;
    [SerializeField] private TMP_Text shotgunShellCountText;
    [SerializeField] private TMP_Text grenadeCountText;
    [SerializeField] private TMP_Text landmineCountText;

    private bool isInventoryOpen = false;
    private Coroutine radialRoutine;
    private CustomButton.CraftableItem hoveredItem;

    private enum IngredientType { Alcohol, Rag, Binding, GunPowder, Can }

    private readonly Dictionary<IngredientType, int> ingredientCounts = new();
    private readonly Dictionary<CustomButton.CraftableItem, int> craftableCounts = new();
    private readonly Dictionary<IngredientType, Image> ingredientImages = new();
    private readonly Dictionary<IngredientType, Image> ingredientRadials = new();
    private readonly Dictionary<CustomButton.CraftableItem, Dictionary<IngredientType, int>> recipes = new();

    private void Start()
    {
        inventory.gameObject.SetActive(false);

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
        UpdateUICounts();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !isInventoryOpen) OpenInventory();
        else if (Input.GetKeyDown(KeyCode.Q) && isInventoryOpen) CloseInventory();
    }

    private void BuildRecipes()
    {
        recipes[CustomButton.CraftableItem.Medikit] = new() { { IngredientType.Alcohol, 1 }, { IngredientType.Rag, 2 }, { IngredientType.Binding, 1 } };
        recipes[CustomButton.CraftableItem.Bandage] = new() { { IngredientType.Rag, 1 }, { IngredientType.Binding, 1 } };
        recipes[CustomButton.CraftableItem.Silencer] = new() { { IngredientType.Can, 1 }, { IngredientType.Rag, 1 }, { IngredientType.Binding, 1 } };
        recipes[CustomButton.CraftableItem.ShotgunShell] = new() { { IngredientType.Can, 1 }, { IngredientType.GunPowder, 1 } };
        recipes[CustomButton.CraftableItem.Grenade] = new() { { IngredientType.Can, 1 }, { IngredientType.GunPowder, 2 }, { IngredientType.Binding, 1 } };
        recipes[CustomButton.CraftableItem.Landmine] = new() { { IngredientType.Can, 1 }, { IngredientType.GunPowder, 2 }, { IngredientType.Binding, 1 } };
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

    private void OpenInventory()
    {
        inventory.gameObject.SetActive(true);
        isInventoryOpen = true;
        OnInventoryToggled?.Invoke(true);
    }

    private void CloseInventory()
    {
        inventory.gameObject.SetActive(false);
        isInventoryOpen = false;
        OnInventoryToggled?.Invoke(false);
    }

    private void OnEnable()
    {
        CustomButton.OnHoveredCraftItem += HandleHoveredCraftItem;
        CustomButton.OnUnhoveredCraftItem += HandleUnhoveredCraftItem;
        CustomButton.OnCraftItemHold += HandleCraftItemHold;
        CustomButton.OnCraftItemRelease += HandleCraftItemRelease;
        CustomButton.OnCraftCompleted += HandleCraftCompleted;
    }

    private void OnDisable()
    {
        if (isInventoryOpen) CloseInventory();

        CustomButton.OnHoveredCraftItem -= HandleHoveredCraftItem;
        CustomButton.OnUnhoveredCraftItem -= HandleUnhoveredCraftItem;
        CustomButton.OnCraftItemHold -= HandleCraftItemHold;
        CustomButton.OnCraftItemRelease -= HandleCraftItemRelease;
        CustomButton.OnCraftCompleted -= HandleCraftCompleted;
    }

    private void HandleHoveredCraftItem(CustomButton.CraftableItem item)
    {
        hoveredItem = item;
        HighlightRecipe(item);
    }

    private void HandleUnhoveredCraftItem(CustomButton.CraftableItem item)
    {
        StopRadialRoutine();
        HideAllRadials();
        ResetIngredientVisuals();
    }

    private void HandleCraftItemHold(CustomButton.CraftableItem item)
    {
        hoveredItem = item;
        HighlightRecipe(item);

        StopRadialRoutine();
        radialRoutine = StartCoroutine(IngredientRadialFillRoutine(item, radialDuration)); // 1.5 sec
    }

    private void HandleCraftItemRelease(CustomButton.CraftableItem item)
    {
        StopRadialRoutine();
        HideAllRadials();
        HighlightRecipe(item);
    }

    private void HandleCraftCompleted(CustomButton.CraftableItem item)
    {
        StopRadialRoutine();
        HideAllRadials();

        if (TryCraft(item))
        {
            UpdateUICounts();
            HighlightRecipe(item);
        }
        else
        {
            Debug.Log($"Not enough ingredients to craft {item}");
        }
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

        foreach (var req in recipe)
        {
            Image radial = ingredientRadials[req.Key];
            if (radial == null) continue;
            radial.fillAmount = 1f;
        }

        radialRoutine = null;
    }

    private bool TryCraft(CustomButton.CraftableItem item)
    {
        if (!recipes.TryGetValue(item, out var recipe)) return false;

        foreach (var req in recipe)
        {
            if (ingredientCounts[req.Key] < req.Value) return false;
        }

        foreach (var req in recipe)
        {
            ingredientCounts[req.Key] -= req.Value;
        }

        craftableCounts[item] += 1;
        return true;
    }

    private void HighlightRecipe(CustomButton.CraftableItem item)
    {
        ResetIngredientVisuals();
        if (!recipes.TryGetValue(item, out var recipe)) return;

        foreach (var req in recipe)
        {
            IngredientType ingredient = req.Key;
            int need = req.Value;
            int have = ingredientCounts[ingredient];

            Image img = ingredientImages[ingredient];
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
            if (ingredientNormalSprite != null) kvp.Value.sprite = ingredientNormalSprite;
            kvp.Value.color = ingredientNormalColor;
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

    private void HideAllRadials()
    {
        foreach (var kvp in ingredientRadials)
        {
            if (kvp.Value == null) continue;
            kvp.Value.fillAmount = 0f;
            kvp.Value.gameObject.SetActive(false);
        }
    }

   
    public void UpdateUICounts()
    {
        // crafting ingredient counts
        if (alcoholCountText) alcoholCountText.text = ingredientCounts[IngredientType.Alcohol].ToString();
        if (ragCountText) ragCountText.text = ingredientCounts[IngredientType.Rag].ToString();
        if (bindingCountText) bindingCountText.text = ingredientCounts[IngredientType.Binding].ToString();
        if (gunPowderCountText) gunPowderCountText.text = ingredientCounts[IngredientType.GunPowder].ToString();
        if (canCountText) canCountText.text = ingredientCounts[IngredientType.Can].ToString();

        // craftable item counts
        if (medikitCountText) medikitCountText.text = craftableCounts[CustomButton.CraftableItem.Medikit].ToString();
        if (bandageCountText) bandageCountText.text = craftableCounts[CustomButton.CraftableItem.Bandage].ToString();
        if (silencerCountText) silencerCountText.text = craftableCounts[CustomButton.CraftableItem.Silencer].ToString();
        if (shotgunShellCountText) shotgunShellCountText.text = craftableCounts[CustomButton.CraftableItem.ShotgunShell].ToString();
        if (grenadeCountText) grenadeCountText.text = craftableCounts[CustomButton.CraftableItem.Grenade].ToString();
        if (landmineCountText) landmineCountText.text = craftableCounts[CustomButton.CraftableItem.Landmine].ToString();
    }

    //  helpers for other scripts
    public void AddIngredient_Alcohol(int amount) { ingredientCounts[IngredientType.Alcohol] += amount; UpdateUICounts(); if (isInventoryOpen) HighlightRecipe(hoveredItem); }
    public void AddIngredient_Rag(int amount) { ingredientCounts[IngredientType.Rag] += amount; UpdateUICounts(); if (isInventoryOpen) HighlightRecipe(hoveredItem); }
    public void AddIngredient_Binding(int amount) { ingredientCounts[IngredientType.Binding] += amount; UpdateUICounts(); if (isInventoryOpen) HighlightRecipe(hoveredItem); }
    public void AddIngredient_GunPowder(int amount) { ingredientCounts[IngredientType.GunPowder] += amount; UpdateUICounts(); if (isInventoryOpen) HighlightRecipe(hoveredItem); }
    public void AddIngredient_Can(int amount) { ingredientCounts[IngredientType.Can] += amount; UpdateUICounts(); if (isInventoryOpen) HighlightRecipe(hoveredItem); }
}