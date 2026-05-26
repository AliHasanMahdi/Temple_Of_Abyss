using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Level05PuzzlePedestal : MonoBehaviour
{
    public string rewardItemId;
    public string puzzleName = "Puzzle";
    public string rewardName = "Gem";
    public Color rewardColor = Color.white;
    public float interactDistance = 3.5f;

    private Transform player;
    private bool solved;

    private void Start()
    {
        solved = !string.IsNullOrEmpty(rewardItemId) && PersistentInventory.IsGemResolved(rewardItemId);
        ApplyColor();
    }

    private void Update()
    {
        if (solved)
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
            HUDManager.Instance.ShowInteractPrompt("Press E to solve " + puzzleName);
        }

        if (near && WasInteractPressed())
        {
            Solve();
        }
        else if (!near && HUDManager.Instance != null)
        {
            HUDManager.Instance.HideInteractPrompt();
        }
    }

    private void Solve()
    {
        solved = true;
        PersistentInventory.Collect(rewardItemId);

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RefreshInventory();
        }

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.ShowInteractPrompt(rewardName + " obtained");
        }
    }

    private void ApplyColor()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = rewardColor;
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
