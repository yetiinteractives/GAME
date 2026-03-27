using UnityEngine;

public interface IInteractable
{

    public void ShowInteractableIcon();
    public void HideInteractableIcon();

    public void ShowInteractionPrompt();
    public void HideInteractionPrompt();

    public void Interact();
}
