using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LootInteraction : MonoBehaviour, IInteractable
{
    [SerializeField]private SpriteRenderer lootIcon;

    private void Start()
    {
        if (lootIcon == null)
        {
            lootIcon = GetComponentInChildren<SpriteRenderer>();

        }

        HideInteractableIcon();

    }

    public void ShowInteractableIcon()
    {
        lootIcon?.gameObject.SetActive(true);
    }
    public void HideInteractableIcon()
    {
        lootIcon.gameObject.SetActive(false);   
    }

    public void ShowInteractionPrompt()
    {
    }
    public void HideInteractionPrompt()
    {

    }

    public void Interact()
    {
    }

  

  
}
