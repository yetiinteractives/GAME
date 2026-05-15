using UnityEngine;

public class ChestInteract : MonoBehaviour, IInteractable
{
    [Header("UI")]
    [SerializeField] private GameObject icon;
    [SerializeField] private GameObject prompt;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;

    private bool isOpen;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        HideInteractableIcon();
        HideInteractionPrompt();
    }

    public void ShowInteractableIcon()
    {
        if (isOpen) return;

        if (icon != null)
            icon.SetActive(true);
    }

    public void HideInteractableIcon()
    {
        if (icon != null)
            icon.SetActive(false);
    }

    public void ShowInteractionPrompt()
    {
        if (isOpen) return;

        if (prompt != null)
            prompt.SetActive(true);
    }

    public void HideInteractionPrompt()
    {
        if (prompt != null)
            prompt.SetActive(false);
    }

    public void Interact()
    {
        if (isOpen) return;

        OpenChest();
    }

    private void OpenChest()
    {
        isOpen = true;

        HideInteractableIcon();
        HideInteractionPrompt();

        if (anim != null)
            anim.SetTrigger("OpenChest");

        if (openSound != null)
            AudioSource.PlayClipAtPoint(openSound, transform.position);
    }
}