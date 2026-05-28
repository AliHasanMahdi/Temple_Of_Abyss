using UnityEngine;
using TMPro;

public class Box : MonoBehaviour
{
    public bool hasKey = false;
    public TMP_Text promptText;

    private bool playerNearby = false;
    private float holdTimer = 0f;
    private float holdDuration = 2f;
    private bool opened = false;

    void Update()
    {
        if (!playerNearby || opened) return;

        if (Input.GetKey(KeyCode.E))
        {
            holdTimer += Time.deltaTime;
            float percent = Mathf.FloorToInt((holdTimer / holdDuration) * 100);
            promptText.text = "Opening... " + percent + "%";

            if (holdTimer >= holdDuration)
            {
                OpenBox();
            }
        }
        else
        {
            if (holdTimer > 0f)
            {
                holdTimer = 0f;
                promptText.text = "Hold E to open box";
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            promptText.text = "Hold E to open box";
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            holdTimer = 0f;
            promptText.text = "";
        }
    }

    void OpenBox()
    {
        opened = true;
        if (hasKey)
        {
            // Add key to hero
            AN_HeroInteractive hero = Object.FindAnyObjectByType<AN_HeroInteractive>();
            if (hero != null) hero.AddKey(true); // true = red key

            // Show in inventory
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.AddKey(true);

            promptText.text = "You found a KEY!";
        }
        else
        {
            promptText.text = "Empty box...";
        }

        // Clear prompt after 2 seconds
        Invoke("ClearPrompt", 2f);
    }

    void ClearPrompt()
    {
        promptText.text = "";
    }
}
