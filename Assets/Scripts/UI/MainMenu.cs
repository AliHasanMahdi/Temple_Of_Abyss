using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Load Game Info")]
    public TMP_Text loadGameText;
    public GameObject settingsPanel;

    void Awake()
    {
        EnsureSaveSystemExists();
        EnsureReferences();
    }

    void Start()
    {
        Time.timeScale = 1f;

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowHUD(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        CheckSaveFile();

        AddButtonListener(newGameButton, NewGame);
        AddButtonListener(loadGameButton, LoadGame);
        AddButtonListener(settingsButton, OpenSettings);
        AddButtonListener(quitButton, QuitGame);
    }

    void EnsureSaveSystemExists()
    {
        if (FindFirstObjectByType<SaveSystem>() != null)
            return;

        new GameObject("SaveSystem").AddComponent<SaveSystem>();
    }

    void EnsureReferences()
    {
        newGameButton ??= FindButton("NewGameButton");
        loadGameButton ??= FindButton("LoadGameButton");
        settingsButton ??= FindButton("SettingsButton");
        quitButton ??= FindButton("QuitButton");
        settingsPanel ??= GameObject.Find("SettingsPanel");

        if (loadGameText == null && loadGameButton != null)
            loadGameText = loadGameButton.GetComponentInChildren<TMP_Text>(true);
    }

    Button FindButton(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        return target != null ? target.GetComponent<Button>() : null;
    }

    void CheckSaveFile()
    {
        if (PlayerPrefs.HasKey("SavedScene"))
        {
            if (loadGameButton != null)
                loadGameButton.interactable = true;

            string savedLevel = PlayerPrefs.GetString("SavedLevelName", "Unknown Level");
            if (loadGameText != null)
                loadGameText.text = "Continue: " + savedLevel;
        }
        else
        {
            if (loadGameButton != null)
                loadGameButton.interactable = false;

            if (loadGameText != null)
                loadGameText.text = "No Save Found";
        }
    }

    public void NewGame()
    {
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.DeleteSave();
        else
            PlayerPrefs.DeleteKey("SavedScene");

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowHUD(true);

        SceneManager.LoadScene("Level01_Entrance");
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("SavedScene"))
            return;

        PlayerHealth.ShouldRestorePosition = true;

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowHUD(true);

        SceneManager.LoadScene(PlayerPrefs.GetString("SavedScene"));
    }

    public void OpenSettings()
    {
        if (SettingsMenu.Instance != null)
        {
            SettingsMenu.Instance.Open();
            return;
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void HideSettings()
    {
        if (SettingsMenu.Instance != null)
        {
            SettingsMenu.Instance.Close();
            return;
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }

    void AddButtonListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        TempleAudio.RegisterButton(button);
        button.onClick.AddListener(action);
    }
}
