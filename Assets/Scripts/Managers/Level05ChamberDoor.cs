using UnityEngine;

public class Level05ChamberDoor : MonoBehaviour, IPlayerInteractable
{
    public string requiredItemId;
    public string doorName = "Door";
    public bool treasureDoor;
    public GameObject treasureObject;
    public float interactDistance = 4f;

    private bool opened;

    private void Start()
    {
        if (treasureDoor)
        {
            if (PersistentInventory.AllOfferingsPlaced())
            {
                Open();
            }
            else if (treasureObject != null)
            {
                treasureObject.SetActive(false);
            }
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        return enabled && gameObject.activeInHierarchy && !opened && !treasureDoor && IsWithinRange(interactor);
    }

    public string GetPromptText()
    {
        return "Press E to unlock " + doorName;
    }

    public void Interact(GameObject interactor)
    {
        TryOpenWithKey();
    }

    public void TryOpenWithKey()
    {
        if (string.IsNullOrEmpty(requiredItemId) || PersistentInventory.Consume(requiredItemId))
        {
            Open();
            return;
        }

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowTimedMessage(doorName + " needs its special key", 2f);
    }

    public void Open()
    {
        if (opened)
            return;

        opened = true;
        if (treasureObject != null)
            treasureObject.SetActive(true);

        gameObject.SetActive(false);

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.RefreshInventory();

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowTimedMessage(doorName + " opened", 2f);
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
