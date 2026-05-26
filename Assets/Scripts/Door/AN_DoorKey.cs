using UnityEngine;

public class AN_DoorKey : MonoBehaviour, IPlayerInteractable
{
    public enum KeyType
    {
        Red,
        Blue
    }

    [Header("Legacy Compatibility")]
    [Tooltip("Older door prefabs serialized this flag instead of keyType.")]
    public bool isRedKey = true;

    [Header("Key Settings")]
    public KeyType keyType;

    [Tooltip("Unique ID for collectible keys so they do not respawn after a saved checkpoint.")]
    public string keyID = string.Empty;

    [Tooltip("How close the player must be to interact with the key.")]
    public float pickupRange = 2f;

    private AN_HeroInteractive hero;
    private AN_DoorScript linkedDoor;
    private Collider interactionCollider;

    bool IsDoorLockKey => linkedDoor != null && string.IsNullOrEmpty(keyID);
    bool IsRedKeyType => keyType == KeyType.Red;

    void Awake()
    {
        linkedDoor = GetComponentInParent<AN_DoorScript>();
        interactionCollider = GetComponent<Collider>() ?? GetComponentInChildren<Collider>();
        SyncLegacyFields();
    }

    void Start()
    {
        hero = Object.FindAnyObjectByType<AN_HeroInteractive>();
        SyncLegacyFields();

        if (hero == null)
            Debug.LogError("[AN_DoorKey] No AN_HeroInteractive found! " + gameObject.name + " won't work.");

        if (!IsDoorLockKey &&
            SaveSystem.Instance != null &&
            !string.IsNullOrEmpty(keyID) &&
            SaveSystem.Instance.IsKeyPickedUp(keyID))
        {
            Debug.Log("[AN_DoorKey] Key already collected and saved - not respawning: " + keyID);
            Destroy(gameObject);
            return;
        }

        if (IsDoorLockKey && linkedDoor != null && !linkedDoor.RequiresKey)
            gameObject.SetActive(false);
    }

    public bool CanInteract(GameObject interactor)
    {
        if (!enabled || !gameObject.activeInHierarchy)
            return false;

        if (IsDoorLockKey)
            return linkedDoor != null && linkedDoor.RequiresKey && linkedDoor.CanInteract(interactor);

        return InRange(interactor);
    }

    public string GetPromptText()
    {
        if (IsDoorLockKey)
            return linkedDoor != null ? linkedDoor.GetPromptText() : "Press E to unlock door";

        return "Press E to collect " + (IsRedKeyType ? "Red Key" : "Blue Key");
    }

    public void Interact(GameObject interactor)
    {
        if (interactor != null)
            hero = interactor.GetComponent<AN_HeroInteractive>() ?? interactor.GetComponentInChildren<AN_HeroInteractive>();

        if (IsDoorLockKey)
        {
            UnlockDoor(interactor);
            return;
        }

        PickUp();
    }

    void PickUp()
    {
        if (hero == null)
            return;

        hero.AddKey(IsRedKeyType);
        InventoryManager.Instance?.AddKey(IsRedKeyType);

        Debug.Log("[AN_DoorKey] " + (IsRedKeyType ? "Red" : "Blue") + " Key picked up!");

        if (SaveSystem.Instance != null && !string.IsNullOrEmpty(keyID))
            SaveSystem.Instance.PendingKeyPickup(keyID, IsRedKeyType);

        HUDManager.Instance?.ShowKeyMessage(IsRedKeyType);
        Destroy(gameObject);
    }

    void UnlockDoor(GameObject interactor)
    {
        if (linkedDoor == null)
            return;

        bool requiredBefore = linkedDoor.RequiresKey;
        linkedDoor.Interact(interactor);

        if (requiredBefore && !linkedDoor.RequiresKey)
            gameObject.SetActive(false);
    }

    void SyncLegacyFields()
    {
        if (keyType == KeyType.Red && !isRedKey)
            keyType = KeyType.Blue;

        isRedKey = IsRedKeyType;
    }

    bool InRange(GameObject interactor)
    {
        if (interactor == null)
            return false;

        Transform origin = interactor.transform;
        PlayerMovement movement = interactor.GetComponent<PlayerMovement>();
        if (movement != null && movement.ViewTransform != null)
            origin = movement.ViewTransform;

        Vector3 keyPosition = interactionCollider != null
            ? interactionCollider.ClosestPoint(origin.position)
            : transform.position;

        return Vector3.Distance(keyPosition, origin.position) < pickupRange;
    }

    void OnDrawGizmosSelected()
    {
        SyncLegacyFields();
        Gizmos.color = IsRedKeyType ? Color.red : Color.blue;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
