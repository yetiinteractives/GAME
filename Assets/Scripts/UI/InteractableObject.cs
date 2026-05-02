using UnityEngine;

public class InteractionObjects : MonoBehaviour, IInteractable
{
    public enum InteractionType
    {
        Door,
        Puzzle
    }

    [SerializeField] private InteractionType interactionType;

    [Header("UI")]
    [SerializeField] private GameObject icon;
    [SerializeField] private GameObject prompt;

    private bool isBusy;

    private void Start()
    {
        HideInteractableIcon();
        HideInteractionPrompt();
    }

    public void ShowInteractableIcon()
    {
        if (icon != null) icon.SetActive(true);
    }

    public void HideInteractableIcon()
    {
        if (icon != null) icon.SetActive(false);
    }

    public void ShowInteractionPrompt()
    {
        if (prompt != null) prompt.SetActive(true);
    }

    public void HideInteractionPrompt()
    {
        if (prompt != null) prompt.SetActive(false);
    }

    public void Interact()
    {
        if (isBusy) return;

        // Later you can check keys, conditions, etc.
        if (!TryInteract()) return;

        ExecuteInteraction();
    }

    private bool TryInteract()
    {
        // Placeholder for future key/level checks
        return true;
    }

    private void ExecuteInteraction()
    {
        isBusy = true;

        switch (interactionType)
        {
            case InteractionType.Door:
                Debug.Log("Door interaction triggered");
                // TODO: open door animation / disable collider
                break;

            case InteractionType.Puzzle:
                Debug.Log("Puzzle interaction triggered");
                // TODO: open puzzle UI / start puzzle logic
                break;
        }

        isBusy = false;
    }
}