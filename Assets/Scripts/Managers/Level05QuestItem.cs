using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Level05QuestItem : MonoBehaviour
{
    public string itemId;
    public string displayName = "Item";
    public Color itemColor = Color.white;
    public float collectDistance = 3f;
    public bool finishGameOnCollect = false;

    private Transform player;
    private bool collected;

    private void Start()
    {
        if (!string.IsNullOrEmpty(itemId) && PersistentInventory.Has(itemId))
        {
            Destroy(gameObject);
            return;
        }

        ApplyColor();
    }

    private void Update()
    {
        if (collected)
        {
            return;
        }

        EnsurePlayer();
        if (player == null)
        {
            return;
        }

        bool near = Vector3.Distance(transform.position, player.position) <= collectDistance;
        if (near && HUDManager.Instance != null)
        {
            HUDManager.Instance.ShowInteractPrompt("Press E to collect " + displayName);
        }

        if (near && WasInteractPressed())
        {
            Collect();
        }
        else if (!near && HUDManager.Instance != null)
        {
            HUDManager.Instance.HideInteractPrompt();
        }
    }

    private void Collect()
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return;
        }

        collected = true;
        PersistentInventory.Collect(itemId);

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RefreshInventory();
        }

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.ShowInteractPrompt(displayName + " collected");
        }

        if (finishGameOnCollect)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
            return;
        }

        Destroy(gameObject);
    }

    private void ApplyColor()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = itemColor;
        }

        Light light = GetComponentInChildren<Light>();
        if (light != null)
        {
            light.color = itemColor;
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
