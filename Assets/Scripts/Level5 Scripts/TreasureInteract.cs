using UnityEngine;

public class TreasureInteract : MonoBehaviour, IPlayerInteractable
{
    private TreasureRoom treasureRoom;
    private bool playerNearby = false;
    private float interactionRange = 3f;

    void Start()
    {
        treasureRoom = FindFirstObjectByType<TreasureRoom>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            if (treasureRoom != null)
                treasureRoom.PlayerNearTreasure(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (treasureRoom != null)
                treasureRoom.PlayerNearTreasure(false);
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        return enabled &&
               gameObject.activeInHierarchy &&
               playerNearby &&
               treasureRoom != null &&
               !treasureRoom.IsCollected &&
               InRange(interactor);
    }

    public string GetPromptText()
    {
        return "Press E to collect the treasure";
    }

    public void Interact(GameObject interactor)
    {
        if (treasureRoom != null)
            treasureRoom.CollectTreasure();
    }

    bool InRange(GameObject interactor)
    {
        if (interactor == null)
            return false;

        Transform origin = interactor.transform;
        PlayerMovement movement = interactor.GetComponent<PlayerMovement>()
            ?? interactor.GetComponentInChildren<PlayerMovement>();
        if (movement != null && movement.ViewTransform != null)
            origin = movement.ViewTransform;

        return Vector3.Distance(transform.position, origin.position) <= interactionRange;
    }
}
