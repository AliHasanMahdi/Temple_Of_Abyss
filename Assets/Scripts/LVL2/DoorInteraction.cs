using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    public KeypadUIManager keypadUI;
    public GameObject doorPrompt;

    private TextMeshProUGUI promptText;
    private bool playerNear = false;
    private Coroutine hideCoroutine;

    void Start()
    {
        if (doorPrompt != null)
        {
            promptText = doorPrompt.GetComponent<TextMeshProUGUI>();
            doorPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (playerNear && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // Hide prompt when opening keypad
            if (doorPrompt != null)
                doorPrompt.SetActive(false);

            keypadUI.OpenKeypad();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = true;

        // Cancel any pending hide
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (doorPrompt != null)
        {
            doorPrompt.SetActive(true);
            if (promptText != null)
                promptText.text = "Press [E]";
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = false;
        hideCoroutine = StartCoroutine(DelayedHide());
    }

    IEnumerator DelayedHide()
    {
        yield return new WaitForSeconds(0.15f);

        if (!playerNear && doorPrompt != null)
            doorPrompt.SetActive(false);

        hideCoroutine = null;
    }
}