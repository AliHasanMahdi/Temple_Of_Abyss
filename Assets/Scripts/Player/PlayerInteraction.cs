using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 4f;
    public LayerMask interactionMask = ~0;
    public float interactionProbeRadius = 0.15f;

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
        if (interactable == null)
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
        RaycastHit[] hits = Physics.SphereCastAll(
            ray,
            interactionProbeRadius,
            interactionRange,
            interactionMask,
            QueryTriggerInteraction.Collide);

        if (hits == null || hits.Length == 0)
            return null;

        Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

        foreach (RaycastHit hit in hits)
        {
            IPlayerInteractable interactable = FindInteractableComponent(hit.collider);
            if (interactable == null)
                continue;

            if (!interactable.CanInteract(gameObject))
                continue;

            return interactable;
        }

        return null;
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
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
    }
}
