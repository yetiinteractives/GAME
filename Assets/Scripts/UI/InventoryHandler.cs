using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryHandler : MonoBehaviour
{
    public static event Action <bool> OnInventoryToggled;

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


    private bool isInventoryOpen = false;

    private void Start()
    {
        inventory.gameObject.SetActive(false);


    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !isInventoryOpen)
        {
            OpenInventory();
        }
        else if (Input.GetKeyDown(KeyCode.Q) && isInventoryOpen)
        {
            CloseInventory();
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
    }
    private void OnDisable()
    {
        if (isInventoryOpen)
        {
            CloseInventory() ;
        }

        CustomButton.OnHoveredCraftItem -= HandleHoveredCraftItem;
        CustomButton.OnUnhoveredCraftItem -= HandleUnhoveredCraftItem;
        CustomButton.OnCraftItemHold -= HandleCraftItemHold;
        CustomButton.OnCraftItemRelease -= HandleCraftItemRelease;

    }

    private void HandleCraftItemRelease(CustomButton.CraftableItem item)
    {
        
    }

    private void HandleCraftItemHold(CustomButton.CraftableItem item)
    {

    }

    private void HandleUnhoveredCraftItem(CustomButton.CraftableItem item)
    {

    }

    private void HandleHoveredCraftItem(CustomButton.CraftableItem item)
    {

    }
}
