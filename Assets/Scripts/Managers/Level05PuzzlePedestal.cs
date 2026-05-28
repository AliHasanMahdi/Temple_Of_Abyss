using UnityEngine;

public class Level05PuzzlePedestal : MonoBehaviour, IPlayerInteractable
{
    public string rewardItemId;
    public string puzzleName = "Puzzle";
    public string rewardName = "Gem";
    public Color rewardColor = Color.white;
    public float interactDistance = 3.5f;

    private bool solved;

    private void Start()
    {
        solved = !string.IsNullOrEmpty(rewardItemId) && PersistentInventory.IsGemResolved(rewardItemId);
        ApplyColor();
    }

    public bool CanInteract(GameObject interactor)
    {
        return enabled && gameObject.activeInHierarchy && !solved && IsWithinRange(interactor);
    }

    public string GetPromptText()
    {
        return "Press E to solve " + puzzleName;
    }

    public void Interact(GameObject interactor)
    {
        Solve();
    }

    private void Solve()
    {
        solved = true;
        PersistentInventory.Collect(rewardItemId);

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.RefreshInventory();

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowTimedMessage(rewardName + " obtained", 2f);
    }

    private void ApplyColor()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
            renderer.material.color = rewardColor;
    }

    private bool IsWithinRange(GameObject interactor)
    {
        if (interactor == null)
            return false;

        Transform origin = interactor.transform;
        PlayerMovement movement = interactor.GetComponent<PlayerMovement>();
        if (movement != null && movement.ViewTransform != null)
            origin = movement.ViewTransform;

        return Vector3.Distance(transform.position, origin.position) <= interactDistance;
    }
}
