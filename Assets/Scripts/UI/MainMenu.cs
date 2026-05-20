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

    [Header("Load Game Info")]
    public TMP_Text loadGameText;

    void Start()
    {
        // Hide HUD when on main menu
        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowHUD(false);

        CheckSaveFile();

        // Connect buttons
        if (newGameButton != null) newGameButton.onClick.AddListener(NewGame);
        if (loadGameButton != null) loadGameButton.onClick.AddListener(LoadGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    void CheckSaveFile()
    {
        if (PlayerPrefs.HasKey("SavedScene"))
        {
            if (loadGameButton != null) loadGameButton.interactable = true;
            string savedLevel = PlayerPrefs.GetString("SavedLevelName", "Unknown Level");
            if (loadGameText != null) loadGameText.text = "Continue: " + savedLevel;
        }
        else
        {
            if (loadGameButton != null) loadGameButton.interactable = false;
            if (loadGameText != null) loadGameText.text = "No Save Found";
        }
    }

    public void NewGame()
    {
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.DeleteSave();

        // Show HUD before entering game
        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowHUD(true);

        // FIXED: correct scene name
        SceneManager.LoadScene("Level01_Entrance");
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("SavedScene"))
        {
            PlayerHealth.ShouldRestorePosition = true;

            if (HUDManager.Instance != null)
                HUDManager.Instance.ShowHUD(true);

            string sceneToLoad = PlayerPrefs.GetString("SavedScene");
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    public void OpenSettings()
    {
        // Works whether SettingsMenu is DontDestroyOnLoad or a panel in this scene
        if (SettingsMenu.Instance != null)
            SettingsMenu.Instance.Open();
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }
}