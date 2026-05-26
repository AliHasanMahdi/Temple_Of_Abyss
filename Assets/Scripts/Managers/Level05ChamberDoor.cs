using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Level05ChamberDoor : MonoBehaviour
{
    public string requiredItemId;
    public string doorName = "Door";
    public bool treasureDoor;
    public GameObject treasureObject;
    public float interactDistance = 4f;

    private Transform player;
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

    private void Update()
    {
        if (opened || treasureDoor)
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
            HUDManager.Instance.ShowInteractPrompt("Press E to unlock " + doorName);
        }

        if (near && WasInteractPressed())
        {
            TryOpenWithKey();
        }
        else if (!near && HUDManager.Instance != null)
        {
            HUDManager.Instance.HideInteractPrompt();
        }
    }

    public void TryOpenWithKey()
    {
        if (string.IsNullOrEmpty(requiredItemId) || PersistentInventory.Consume(requiredItemId))
        {
            Open();
            return;
        }

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.ShowInteractPrompt(doorName + " needs its special key");
        }
    }

    public void Open()
    {
        if (opened)
        {
            return;
        }

        opened = true;
        if (treasureObject != null)
        {
            treasureObject.SetActive(true);
        }

        gameObject.SetActive(false);

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RefreshInventory();
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
