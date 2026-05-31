using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 4f;
    public LayerMask interactionMask = ~0;
    public float interactionProbeRadius = 0.15f;
    public float interactionLoseGrace = 0.1f;

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PauseMenu pauseMenu;
    private InventoryManager inventoryManager;
    private IPlayerInteractable currentInteractable;
    private float lastInteractableSeenTime;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void OnDisable()
    {
        currentInteractable = null;

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

        bool currentIsValid = CanUseInteractable(currentInteractable);
        IPlayerInteractable interactable = FindInteractable(currentIsValid ? currentInteractable : null);

        if (interactable != null)
        {
            currentInteractable = interactable;
            lastInteractableSeenTime = Time.unscaledTime;
        }
        else if (currentInteractable == null ||
                 Time.unscaledTime - lastInteractableSeenTime > interactionLoseGrace ||
                 !CanUseInteractable(currentInteractable))
        {
            currentInteractable = null;
            HidePrompt();
            return;
        }
        else
        {
            interactable = currentInteractable;
        }

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowInteractionPrompt(GetPromptText(interactable));

        if (WasInteractPressedThisFrame())
        {
            interactable.Interact(gameObject);

            if (!CanUseInteractable(interactable))
            {
                currentInteractable = null;
                HidePrompt();
            }
        }
    }

    IPlayerInteractable FindInteractable(IPlayerInteractable preferredInteractable)
    {
        Ray ray = new Ray(playerMovement.ViewTransform.position, playerMovement.ViewTransform.forward);
        IPlayerInteractable directHitInteractable = FindInteractableFromHits(
            Physics.RaycastAll(ray, interactionRange, interactionMask, QueryTriggerInteraction.Collide),
            preferredInteractable);

        if (directHitInteractable != null)
            return directHitInteractable;

        return FindInteractableFromHits(
            Physics.SphereCastAll(
                ray,
                interactionProbeRadius,
                interactionRange,
                interactionMask,
                QueryTriggerInteraction.Collide),
            preferredInteractable);
    }

    IPlayerInteractable FindInteractableFromHits(RaycastHit[] hits, IPlayerInteractable preferredInteractable)
    {
        if (hits == null || hits.Length == 0)
            return null;

        Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

        IPlayerInteractable fallbackInteractable = null;
        foreach (RaycastHit hit in hits)
        {
            IPlayerInteractable interactable = FindInteractableComponent(hit.collider);
            if (interactable == null || !CanUseInteractable(interactable))
                continue;

            if (preferredInteractable != null && ReferenceEquals(interactable, preferredInteractable))
                return interactable;

            fallbackInteractable ??= interactable;
        }

        return fallbackInteractable;
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

    bool CanUseInteractable(IPlayerInteractable interactable)
    {
        if (!IsInteractableAlive(interactable))
            return false;

        try
        {
            return interactable.CanInteract(gameObject);
        }
        catch (MissingReferenceException)
        {
            return false;
        }
    }

    string GetPromptText(IPlayerInteractable interactable)
    {
        if (!IsInteractableAlive(interactable))
            return string.Empty;

        try
        {
            return interactable.GetPromptText();
        }
        catch (MissingReferenceException)
        {
            return string.Empty;
        }
    }

    bool IsInteractableAlive(IPlayerInteractable interactable)
    {
        if (interactable == null)
            return false;

        if (interactable is UnityEngine.Object unityObject)
            return unityObject != null;

        return true;
    }

    bool WasInteractPressedThisFrame()
    {
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
    }
}
