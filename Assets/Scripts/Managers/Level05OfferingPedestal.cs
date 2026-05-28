using UnityEngine;

public class Level05OfferingPedestal : MonoBehaviour, IPlayerInteractable
{
    public string requiredGemId;
    public string placedId;
    public string displayName = "Gem";
    public Color placedColor = Color.white;
    public float interactDistance = 3.5f;

    private bool placed;

    private void Start()
    {
        placed = !string.IsNullOrEmpty(placedId) && PersistentInventory.Has(placedId);
        ApplyVisual();
    }

    public bool CanInteract(GameObject interactor)
    {
        return enabled && gameObject.activeInHierarchy && !placed && IsWithinRange(interactor);
    }

    public string GetPromptText()
    {
        return "Press E to place " + displayName;
    }

    public void Interact(GameObject interactor)
    {
        TryPlace();
    }

    private void TryPlace()
    {
        if (!PersistentInventory.Consume(requiredGemId))
        {
            if (HUDManager.Instance != null)
                HUDManager.Instance.ShowTimedMessage("You need the " + displayName, 2f);

            return;
        }

        placed = true;
        PersistentInventory.MarkPlaced(placedId);
        ApplyVisual();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.RefreshInventory();

        if (PersistentInventory.AllOfferingsPlaced())
        {
            Level05ChamberDoor[] doors = Object.FindObjectsByType<Level05ChamberDoor>(FindObjectsSortMode.None);
            foreach (Level05ChamberDoor door in doors)
            {
                if (door != null && door.treasureDoor)
                    door.Open();
            }
        }

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowTimedMessage(displayName + " placed", 2f);
    }

    private void ApplyVisual()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
            renderer.material.color = placed ? placedColor : Color.gray;
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
