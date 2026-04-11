using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 4f;
    public LayerMask interactionMask = ~0;

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PauseMenu pauseMenu;

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

        if (pauseMenu != null && pauseMenu.IsPaused)
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

        Interactable interactable = FindInteractable();
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

    Interactable FindInteractable()
    {
        Ray ray = new Ray(playerMovement.ViewTransform.position, playerMovement.ViewTransform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionMask, QueryTriggerInteraction.Collide))
            return null;

        return hit.collider.GetComponentInParent<Interactable>();
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
