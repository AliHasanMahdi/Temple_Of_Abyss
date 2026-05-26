using UnityEngine;

public interface IPlayerInteractable
{
    bool CanInteract(GameObject interactor);
    string GetPromptText();
    void Interact(GameObject interactor);
}
