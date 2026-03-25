using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryHandler : MonoBehaviour
{
    public static event Action <bool> OnInventoryToggled;

    [SerializeField] private Image inventory;

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

    private void OnDisable()
    {
        if (isInventoryOpen)
        {
            CloseInventory() ;
        }
    }

}
