using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TreasureRoom : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text promptText;
    public GameObject victoryPanel;
    public string mainMenuSceneName = "MainMenu"; // must match your scene name exactly

    [Header("Treasure Object")]
    public GameObject treasureObject; // drag your treasure chest/object here

    private bool playerInRoom = false;
    private bool playerNearTreasure = false;
    private bool collected = false;

    void Start()
    {
        victoryPanel.SetActive(false);
        promptText.text = "";
    }

    void Update()
    {
        if (collected) return;

        if (Input.GetKeyDown(KeyCode.E) && playerNearTreasure)
        {
            CollectTreasure();
        }
    }

    // Called when player enters the ROOM trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRoom = true;
            promptText.text = "You found the Treasure Room!\npress E to collect!";
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRoom = false;
            if (!playerNearTreasure)
                promptText.text = "";
        }
    }

    public void PlayerNearTreasure(bool isNear)
    {
        playerNearTreasure = isNear;
        if (isNear)
            promptText.text = "Press E to collect the Treasure!";
        else if (playerInRoom)
            promptText.text = "You found the Treasure Room!\npress E to collect!";
        else
            promptText.text = "";
    }

    void CollectTreasure()
    {
        collected = true;
        promptText.text = "";
        victoryPanel.SetActive(true);

        // Freeze the game
        Time.timeScale = 0f;

        // Show cursor for the button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // reset time before loading
        SceneManager.LoadScene(mainMenuSceneName);
    }
}