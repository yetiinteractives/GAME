using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LootInteraction : MonoBehaviour, IInteractable
{
    [SerializeField]private GameObject lootIcon;
    [SerializeField] private GameObject promptText;

    private void Start()
    {
        

        HideInteractableIcon();
        HideInteractionPrompt();

    }

    public void ShowInteractableIcon()
    {
        lootIcon?.SetActive(true);
    }
    public void HideInteractableIcon()
    {
        lootIcon?.SetActive(false);   
    }

    public void ShowInteractionPrompt()
    {
        promptText?.SetActive(true);
    }
    public void HideInteractionPrompt()
    {
        promptText.SetActive(false);
    }

    public void Interact()
    {
        gameObject.SetActive(false);
    }

  

  
}
