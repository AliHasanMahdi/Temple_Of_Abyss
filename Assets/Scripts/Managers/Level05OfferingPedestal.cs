using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Level05OfferingPedestal : MonoBehaviour
{
    public string requiredGemId;
    public string placedId;
    public string displayName = "Gem";
    public Color placedColor = Color.white;
    public float interactDistance = 3.5f;

    private Transform player;
    private bool placed;

    private void Start()
    {
        placed = !string.IsNullOrEmpty(placedId) && PersistentInventory.Has(placedId);
        ApplyVisual();
    }

    private void Update()
    {
        if (placed)
        {
            return;
        }

        EnsurePlayer();
        if (player == null)
        {
            return;
        }

        bool near = Vector3.Distance(transform.position, player.position) <= interactDistance;
        if (near && HUDManager.Instance != null)
        {
            HUDManager.Instance.ShowInteractPrompt("Press E to place " + displayName);
        }

        if (near && WasInteractPressed())
        {
            TryPlace();
        }
        else if (!near && HUDManager.Instance != null)
        {
            HUDManager.Instance.HideInteractPrompt();
        }
    }

    private void TryPlace()
    {
        if (!PersistentInventory.Consume(requiredGemId))
        {
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.ShowInteractPrompt("You need the " + displayName);
            }

            return;
        }

        placed = true;
        PersistentInventory.MarkPlaced(placedId);
        ApplyVisual();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RefreshInventory();
        }

        if (PersistentInventory.AllOfferingsPlaced())
        {
            Level05ChamberDoor[] doors = Object.FindObjectsOfType<Level05ChamberDoor>();
            foreach (Level05ChamberDoor door in doors)
            {
                if (door != null && door.treasureDoor)
                {
                    door.Open();
                }
            }
        }
    }

    private void ApplyVisual()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = placed ? placedColor : Color.gray;
        }
    }

    private void EnsurePlayer()
    {
        if (player != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private bool WasInteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.E);
#else
        return false;
#endif
    }
}
