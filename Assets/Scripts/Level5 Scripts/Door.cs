using UnityEngine;
using TMPro;

public class Door : MonoBehaviour
{
    public bool isTreasureRoom = false;
    public TMP_Text promptText;

    private bool playerNearby = false;
    private bool isUnlocked = false;

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
            TryOpen();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            promptText.text = isUnlocked ? "Press E to enter" : "Locked! Press E to use a key.";
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            promptText.text = "";
        }
    }

    void TryOpen()
    {
        if (isUnlocked)
        {
            if (isTreasureRoom)
                GameManager.instance.Win();
        }
        else if (GameManager.instance.HasKey())
        {
            GameManager.instance.UseKey();

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.RemoveAnyKey();

            isUnlocked = true;
            promptText.text = "Unlocked! Press E to enter.";
        }
        else
        {
            promptText.text = "No keys! Search the boxes.";
        }
    }
}
