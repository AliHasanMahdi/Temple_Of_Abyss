using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    [Header("Panels")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;

    [Header("Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button restartButton;
    public Button mainMenuButton;

    public bool IsPaused => isPaused;

    private bool isPaused;
    private bool listenersBound;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureReferences();
    }

    void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        BindButtons();
    }

    void Update()
    {
        if (!WasPausePressedThisFrame())
            return;

        if (isPaused)
            Resume();
        else
            Pause();
    }

    void EnsureReferences()
    {
        pauseMenuPanel ??= GameObject.Find("PauseMenuPanel");
        settingsPanel ??= GameObject.Find("SettingsPanel");
        resumeButton ??= FindButton("ResumeButton");
        settingsButton ??= FindButton("SettingsButton");
        restartButton ??= FindButton("RestartButton");
        mainMenuButton ??= FindButton("MainMenuButton");
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

        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        listenersBound = true;
    }

    public void Pause()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    bool WasPausePressedThisFrame()
    {
        if (Keyboard.current != null)
            return Keyboard.current.escapeKey.wasPressedThisFrame;

        return Input.GetKeyDown(KeyCode.Escape);
    }
}
