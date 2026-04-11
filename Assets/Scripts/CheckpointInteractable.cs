using UnityEngine;

public class CheckpointInteractable : Interactable
{
    private bool isActivated;

    public override bool CanInteract(GameObject interactor)
    {
        return base.CanInteract(interactor) && !isActivated;
    }

    public override string GetPromptText()
    {
        return isActivated ? "Checkpoint Activated" : base.GetPromptText();
    }

    public override void Interact(GameObject interactor)
    {
        if (isActivated)
            return;

        isActivated = true;

        if (SaveSystem.Instance != null)
            SaveSystem.Instance.SaveGame();

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowCheckpointMessage();

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = Color.green;
    }
}
