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

    [Header("Panels")]
    public GameObject settingsPanel;

    [Header("Load Game Info")]
    public TMP_Text loadGameText;

    private bool listenersBound;

    void Awake()
    {
        EnsureSaveSystemExists();
        EnsureReferences();
    }

    void Start()
    {
        BindButtons();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        CheckSaveFile();
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

    void BindButtons()
    {
        if (listenersBound)
            return;

        if (newGameButton != null)
            newGameButton.onClick.AddListener(NewGame);

        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(LoadGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        listenersBound = true;
    }

    void CheckSaveFile()
    {
        if (loadGameButton == null)
            return;

        if (PlayerPrefs.HasKey("SavedScene"))
        {
            loadGameButton.interactable = true;
            string savedLevel = PlayerPrefs.GetString("SavedLevelName", "Unknown Level");

            if (loadGameText != null)
                loadGameText.text = "Continue: " + savedLevel;
        }
        else
        {
            loadGameButton.interactable = false;

            if (loadGameText != null)
                loadGameText.text = "No Save Found";
        }
    }

    public void NewGame()
    {
        PlayerPrefs.DeleteKey("SavedScene");
        PlayerPrefs.DeleteKey("SavedLevelName");
        PlayerPrefs.DeleteKey("SavedScore");
        PlayerPrefs.Save();

        SceneManager.LoadScene("Level01_Entrance");
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("SavedScene"))
            return;

        string sceneToLoad = PlayerPrefs.GetString("SavedScene");
        SceneManager.LoadScene(sceneToLoad);
    }

    public void ShowSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void HideSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }
}
