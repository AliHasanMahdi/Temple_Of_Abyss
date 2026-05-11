using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Panels")]
    public GameObject settingsPanel;

    [Header("Load Game Info")]
    public TMP_Text loadGameText; // shows level name under Load button

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        CheckSaveFile();
    }

    void CheckSaveFile()
    {
        // Check if a save file exists
        if (PlayerPrefs.HasKey("SavedScene"))
        {
            // Save exists — enable Load button
            loadGameButton.interactable = true;

            string savedScene = PlayerPrefs.GetString("SavedScene");
            string savedLevel = PlayerPrefs.GetString("SavedLevelName", "Unknown Level");

            if (loadGameText != null)
                loadGameText.text = "Continue: " + savedLevel;
        }
        else
        {
            // No save — grey out Load button
            loadGameButton.interactable = false;

            if (loadGameText != null)
                loadGameText.text = "No Save Found";
        }
    }

    public void NewGame()
    {
        // Clear any old save data
        PlayerPrefs.DeleteKey("SavedScene");
        PlayerPrefs.DeleteKey("SavedLevelName");
        PlayerPrefs.DeleteKey("SavedScore");
        PlayerPrefs.Save();

        // Load first level
        SceneManager.LoadScene("Level01");
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("SavedScene"))
        {
            string sceneToLoad = PlayerPrefs.GetString("SavedScene");
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    public void ShowSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void HideSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }
}
