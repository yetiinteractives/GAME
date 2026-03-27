using System.Diagnostics;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LootInteraction : MonoBehaviour, IInteractable
{

    public InteractablesEnum interactables;
    public int amount = 10;

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
        TryInteract(); 
        gameObject.SetActive(false);
    }

    public void TryInteract()
    {
        switch (interactables)
        {
                case InteractablesEnum.pistolAmmo:
                    ResourceManager.Instance.SetPistolAmmo(amount);
                    break;
                case InteractablesEnum.shotgunAmmo:
                    ResourceManager.Instance.SetShotgunAmmo(amount);
                    break;
                case InteractablesEnum.sniperAmmo:
                    ResourceManager.Instance.SetSniperAmmo(amount);
                    break;
                case InteractablesEnum.grenade:
                    ResourceManager.Instance.SetGrenade(amount);
                    break;
                case InteractablesEnum.landmine:
                    ResourceManager.Instance.SetLandmine(amount);
                    break;
                case InteractablesEnum.medkit:
                    ResourceManager.Instance.SetMedkit(amount);
                    break;
                case InteractablesEnum.bandage:
                    ResourceManager.Instance.SetBandage(amount);
                    break;
                case InteractablesEnum.silencer:
                    ResourceManager.Instance.SetSilencer(amount);
                    break;
                case InteractablesEnum.alchohol:
                    ResourceManager.Instance.SetAlchohol(amount);
                    break;
                case InteractablesEnum.rag:
                    ResourceManager.Instance.SetRag(amount);
                break;
                case InteractablesEnum.binding:
                    ResourceManager.Instance.SetBinding(amount);
                    break;
                case InteractablesEnum.gunpowder:
                    ResourceManager.Instance.SetGunpowder(amount);
                break;
                case InteractablesEnum.can:
                    ResourceManager.Instance.SetCan(amount);
                    break;
                }
                


        }
    }

    

  

  

