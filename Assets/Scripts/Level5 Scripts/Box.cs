using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Box : MonoBehaviour, IPlayerInteractable
{
    public bool hasKey = false;
    public TMP_Text promptText;
    public float interactionRange = 3f;

    private bool playerNearby = false;
    private float holdTimer = 0f;
    private float holdDuration = 2f;
    private bool opened = false;
    private float lastFocusTime = float.NegativeInfinity;
    private const float FocusGrace = 0.12f;

    void Update()
    {
        if (!playerNearby || opened) return;

        bool usingPromptPath = !UseLegacyInteraction();
        bool canHoldOpen = usingPromptPath ? IsFocused() : true;

        if (Keyboard.current != null && Keyboard.current.eKey.isPressed && canHoldOpen)
        {
            holdTimer += Time.deltaTime;
            ShowLocalPrompt("Opening... " + Mathf.FloorToInt((holdTimer / holdDuration) * 100) + "%");

            if (holdTimer >= holdDuration)
                OpenBox();
        }
        else
        {
            if (holdTimer > 0f)
            {
                holdTimer = 0f;
                ShowLocalPrompt("Hold E to open box");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            ShowLocalPrompt("Hold E to open box");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            holdTimer = 0f;
            lastFocusTime = float.NegativeInfinity;
            ShowLocalPrompt("");
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        if (opened || !playerNearby || !InRange(interactor))
            return false;

        lastFocusTime = Time.unscaledTime;
        return true;
    }

    public string GetPromptText()
    {
        if (holdTimer > 0f)
            return "Hold E to open box (" + Mathf.FloorToInt((holdTimer / holdDuration) * 100) + "%)";

        return "Hold E to open box";
    }

    public void Interact(GameObject interactor)
    {
        // Hold progress is handled in Update while the interact key stays pressed.
    }

    void OpenBox()
    {
        opened = true;
        if (hasKey)
        {
            // Add key to hero
            AN_HeroInteractive hero = FindObjectOfType<AN_HeroInteractive>();
            if (hero != null) hero.RedKey = true;

            // Show in inventory
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.AddKey(true);

            if (GameManager.instance != null)
                GameManager.instance.KeyFound();

            ShowLocalPrompt("You found a KEY!");
        }
        else
        {
            ShowLocalPrompt("Empty box...");
        }

        // Clear prompt after 2 seconds
        Invoke("ClearPrompt", 2f);
    }

    void ClearPrompt()
    {
        ShowLocalPrompt("");
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

        return Vector3.Distance(transform.position, origin.position) <= interactionRange;
    }

    bool IsFocused()
    {
        return Time.unscaledTime - lastFocusTime <= FocusGrace;
    }

    bool UseLegacyInteraction()
    {
        return Object.FindFirstObjectByType<PlayerInteraction>() == null;
    }

    void ShowLocalPrompt(string message)
    {
        if (!UseLegacyInteraction() || promptText == null)
            return;

        promptText.text = message;
    }
}
