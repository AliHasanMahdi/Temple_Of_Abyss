using UnityEngine;

public class Level04VaultPuzzleInteractable : Interactable
{
    public enum PuzzleAction
    {
        Increase,
        Decrease,
        Submit
    }

    public Level04VaultCodePuzzle puzzle;
    public PuzzleAction action;
    public int slotIndex = -1;
    public float interactionDistance = 3f;

    public override bool CanInteract(GameObject interactor)
    {
        if (!base.CanInteract(interactor) || puzzle == null)
            return false;

        return IsWithinDistance(interactor, interactionDistance);
    }

    public override void Interact(GameObject interactor)
    {
        if (puzzle == null)
            return;

        switch (action)
        {
            case PuzzleAction.Increase:
                puzzle.IncreaseSlot(slotIndex);
                break;
            case PuzzleAction.Decrease:
                puzzle.DecreaseSlot(slotIndex);
                break;
            default:
                puzzle.SubmitGuess();
                break;
        }
    }
}
