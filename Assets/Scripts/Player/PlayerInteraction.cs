using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 4f;
    public LayerMask interactionMask = ~0;

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PauseMenu pauseMenu;
    private InventoryManager inventoryManager;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void OnDisable()
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.HideInteractionPrompt();
    }

    void Update()
    {
        pauseMenu ??= FindFirstObjectByType<PauseMenu>();
        inventoryManager ??= FindFirstObjectByType<InventoryManager>();

        if ((pauseMenu != null && pauseMenu.IsPaused) ||
            (inventoryManager != null && inventoryManager.IsOpen) ||
            Time.timeScale == 0f)
        {
            HidePrompt();
            return;
        }

        if (playerMovement == null || playerMovement.ViewTransform == null)
        {
            HidePrompt();
            return;
        }

        if (playerHealth != null && playerHealth.IsDead)
        {
            HidePrompt();
            return;
        }

        IPlayerInteractable interactable = FindInteractable();
        if (interactable == null || !interactable.CanInteract(gameObject))
        {
            HidePrompt();
            return;
        }

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowInteractionPrompt(interactable.GetPromptText());

        if (WasInteractPressedThisFrame())
            interactable.Interact(gameObject);
    }

    IPlayerInteractable FindInteractable()
    {
        Ray ray = new Ray(playerMovement.ViewTransform.position, playerMovement.ViewTransform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionMask, QueryTriggerInteraction.Collide))
            return null;

        return FindInteractableComponent(hit.collider);
    }

    IPlayerInteractable FindInteractableComponent(Collider hitCollider)
    {
        Transform current = hitCollider != null ? hitCollider.transform : null;
        while (current != null)
        {
            MonoBehaviour[] components = current.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                if (component is Behaviour behaviour && !behaviour.enabled)
                    continue;

                if (component is IPlayerInteractable interactable)
                    return interactable;
            }

            current = current.parent;
        }

        return null;
    }

    void HidePrompt()
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.HideInteractionPrompt();
    }

    bool WasInteractPressedThisFrame()
    {
        if (Keyboard.current != null)
            return Keyboard.current.eKey.wasPressedThisFrame;

        return Input.GetKeyDown(KeyCode.E);
    }
}
