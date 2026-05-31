using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AN_DoorKey : MonoBehaviour, IPlayerInteractable
{
    const float MinimumPickupTriggerRadius = 0.9f;

    public enum KeyType
    {
        Red,
        Blue
    }

    [Header("Key Settings")]
    public KeyType keyType;

    [Tooltip("Unique ID for this key — used to stop it respawning after death")]
    public string keyID = "Key_01";

    [Tooltip("How close the player must be to pick up the key")]
    public float pickupRange = 2f;

    private AN_HeroInteractive hero;

    void Awake()
    {
        EnsurePickupCollider();
    }

    void Start()
    {
        hero = Object.FindAnyObjectByType<AN_HeroInteractive>();

        if (hero == null)
            Debug.LogError("[AN_DoorKey] No AN_HeroInteractive found! " + gameObject.name + " won't work.");

        // Only destroy (don't respawn) if the pickup was confirmed by a checkpoint save on disk.
        // If the player picked it up but died before a checkpoint, IsKeyPickedUp returns false
        // (pending memory was cleared on scene reload) so the key correctly respawns.
        if (SaveSystem.Instance != null && SaveSystem.Instance.IsKeyPickedUp(keyID))
        {
            Debug.Log("[AN_DoorKey] Key already collected and saved — not respawning: " + keyID);
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!UseLegacyInteraction()) return;
        hero ??= Object.FindAnyObjectByType<AN_HeroInteractive>();
        if (hero == null) return;
        if (!InRange()) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            PickUp();
    }

    public bool CanInteract(GameObject interactor)
    {
        return enabled &&
               gameObject.activeInHierarchy &&
               ResolveHero(interactor) != null &&
               InRange(interactor);
    }

    public string GetPromptText()
    {
        return keyType == KeyType.Red
            ? "Press E to pick up red key"
            : "Press E to pick up blue key";
    }

    public void Interact(GameObject interactor)
    {
        if (ResolveHero(interactor) == null)
            return;

        PickUp();
    }

    void PickUp()
    {
        switch (keyType)
        {
            case KeyType.Red:
                hero.RedKey = true;
                Debug.Log("[AN_DoorKey] Red Key picked up!");
                break;
            case KeyType.Blue:
                hero.BlueKey = true;
                Debug.Log("[AN_DoorKey] Blue Key picked up!");
                break;
        }

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.AddKey(keyType == KeyType.Red);

        // Held in memory only — flushed to disk when player touches a checkpoint.
        // If the player dies before that, the scene reloads, pending memory is gone,
        // and this key will respawn correctly.
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.PendingKeyPickup(keyID, keyType == KeyType.Red);

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowKeyMessage(keyType == KeyType.Red);

        Destroy(gameObject);
    }

    bool InRange()
    {
        if (Camera.main == null) return false;
        return Vector3.Distance(
            transform.position,
            Camera.main.transform.position
        ) < pickupRange;
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

        return Vector3.Distance(transform.position, origin.position) < pickupRange;
    }

    bool UseLegacyInteraction()
    {
        return Object.FindFirstObjectByType<PlayerInteraction>() == null;
    }

    AN_HeroInteractive ResolveHero(GameObject interactor)
    {
        if (hero != null)
            return hero;

        if (interactor != null)
        {
            hero = interactor.GetComponent<AN_HeroInteractive>()
                ?? interactor.GetComponentInChildren<AN_HeroInteractive>();
        }

        if (hero == null)
            hero = Object.FindAnyObjectByType<AN_HeroInteractive>();

        return hero;
    }

    void EnsurePickupCollider()
    {
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
            sphereCollider = gameObject.AddComponent<SphereCollider>();

        sphereCollider.isTrigger = true;
        sphereCollider.radius = Mathf.Max(sphereCollider.radius, MinimumPickupTriggerRadius);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = keyType == KeyType.Red ? Color.red : Color.blue;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
