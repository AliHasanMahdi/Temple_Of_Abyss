using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    [Header("Panels")]
    [FormerlySerializedAs("pausePanel")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;

    [Header("Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button restartButton;
    public Button mainMenuButton;

    public bool IsPaused => isPaused;

    bool isPaused;
    CanvasGroup pauseCanvasGroup;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureReferences();
        EnsureCanvas();
        EnsureCanvasGroup();
    }

    void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SetPausePanelVisible(false);

        AddButtonListener(resumeButton, Resume);
        AddButtonListener(settingsButton, OpenSettings);
        AddButtonListener(restartButton, RestartLevel);
        AddButtonListener(mainMenuButton, GoToMainMenu);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "MainMenu" || scene == "GameOver")
            return;

        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (SettingsMenu.Instance != null && SettingsMenu.Instance.IsOpen)
        {
            SettingsMenu.Instance.Close();
            return;
        }

        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        SetPausePanelVisible(true);

        PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
        if (movement != null)
            movement.OnPause();
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SetPausePanelVisible(false);

        if (SettingsMenu.Instance != null)
            SettingsMenu.Instance.Close();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
        if (movement != null)
            movement.OnResume();
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

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SetPausePanelVisible(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SetPausePanelVisible(false);

        if (SettingsMenu.Instance != null)
            SettingsMenu.Instance.Close();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        SceneManager.LoadScene("MainMenu");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureReferences();
        EnsureCanvasGroup();
        isPaused = false;
        Time.timeScale = 1f;
        SetPausePanelVisible(false);
    }

    void EnsureReferences()
    {
        pauseMenuPanel ??= GameObject.Find("PauseMenuPanel");
        settingsPanel ??= GameObject.Find("SettingsPanel");
        resumeButton ??= FindButton("ResumeButton");
        settingsButton ??= FindButton("SettingsButton");
        restartButton ??= FindButton("RestartButton");
        mainMenuButton ??= FindButton("MainMenuButton");

        if (pauseMenuPanel == null)
            pauseMenuPanel = gameObject;
    }

    Button FindButton(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        return target != null ? target.GetComponent<Button>() : null;
    }

    void EnsureCanvas()
    {
        if (GetComponentInParent<Canvas>() != null)
            return;

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();
    }

    void EnsureCanvasGroup()
    {
        if (pauseMenuPanel == null)
            return;

        pauseCanvasGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
        if (pauseCanvasGroup == null)
            pauseCanvasGroup = pauseMenuPanel.AddComponent<CanvasGroup>();
    }

    void SetPausePanelVisible(bool visible)
    {
        EnsureReferences();
        EnsureCanvasGroup();

        if (pauseMenuPanel == null)
            return;

        if (!pauseMenuPanel.activeSelf)
            pauseMenuPanel.SetActive(true);

        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = visible ? 1f : 0f;
            pauseCanvasGroup.interactable = visible;
            pauseCanvasGroup.blocksRaycasts = visible;
        }
    }

    void AddButtonListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        TempleAudio.RegisterButton(button);
        button.onClick.AddListener(action);
    }
}
