using UnityEngine;

public class Level05QuestItem : MonoBehaviour, IPlayerInteractable
{
    public string itemId;
    public string displayName = "Item";
    public Color itemColor = Color.white;
    public float collectDistance = 3f;
    public bool finishGameOnCollect = false;

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

    public bool CanInteract(GameObject interactor)
    {
        return enabled &&
               gameObject.activeInHierarchy &&
               !collected &&
               !string.IsNullOrEmpty(itemId) &&
               IsWithinRange(interactor);
    }

    public string GetPromptText()
    {
        return "Press E to collect " + displayName;
    }

    public void Interact(GameObject interactor)
    {
        Collect();
    }

    private void Collect()
    {
        if (string.IsNullOrEmpty(itemId))
            return;

        collected = true;
        PersistentInventory.Collect(itemId);

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.RefreshInventory();

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowTimedMessage(displayName + " collected", 2f);

        if (finishGameOnCollect)
        {
            GameOverMenu.ShowFinish();
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
            return;
        }

        Destroy(gameObject);
    }

    private void ApplyColor()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
            renderer.material.color = itemColor;

        Light light = GetComponentInChildren<Light>();
        if (light != null)
            light.color = itemColor;
    }

    private bool IsWithinRange(GameObject interactor)
    {
        if (interactor == null)
            return false;

        Transform origin = interactor.transform;
        PlayerMovement movement = interactor.GetComponent<PlayerMovement>();
        if (movement != null && movement.ViewTransform != null)
            origin = movement.ViewTransform;

        return Vector3.Distance(transform.position, origin.position) <= collectDistance;
    }
}
