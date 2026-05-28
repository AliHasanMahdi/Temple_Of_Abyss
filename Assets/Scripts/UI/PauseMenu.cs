using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;

    [Header("Panels")]
    [FormerlySerializedAs("pausePanel")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;

    [Header("Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button restartButton;
    public Button mainMenuButton;

    private bool isPaused = false;
    private CanvasGroup pauseCanvasGroup;
    public bool IsPaused => isPaused;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureCanvas();
        EnsurePanelReference();
        EnsureCanvasGroup();
    }

    void Start()
    {
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
        // Only allow ESC pause in game levels, not on menu screens
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "MainMenu" || scene == "GameOver") return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (SettingsMenu.Instance != null && SettingsMenu.Instance.IsOpen)
            {
                SettingsMenu.Instance.Close();
                return;
            }

            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        SetPausePanelVisible(true);

        PlayerMovement pm = FindObjectOfType<PlayerMovement>();
        if (pm != null) pm.OnPause();
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SetPausePanelVisible(false);
        if (SettingsMenu.Instance != null) SettingsMenu.Instance.Close();

        PlayerMovement pm = FindObjectOfType<PlayerMovement>();
        if (pm != null) pm.OnResume();
    }

    public void OpenSettings()
    {
        if (SettingsMenu.Instance != null)
            SettingsMenu.Instance.Open();
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
        if (SettingsMenu.Instance != null) SettingsMenu.Instance.Close();
        SceneManager.LoadScene("MainMenu");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isPaused = false;
        Time.timeScale = 1f;
        SetPausePanelVisible(false);
    }

    void EnsurePanelReference()
    {
        if (pauseMenuPanel == null)
            pauseMenuPanel = gameObject;
    }

    void EnsureCanvasGroup()
    {
        if (pauseMenuPanel == null) return;

        pauseCanvasGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
        if (pauseCanvasGroup == null)
            pauseCanvasGroup = pauseMenuPanel.AddComponent<CanvasGroup>();
    }

    void EnsureCanvas()
    {
        if (GetComponentInParent<Canvas>() != null) return;

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();
    }

    void SetPausePanelVisible(bool visible)
    {
        EnsurePanelReference();
        EnsureCanvasGroup();

        if (pauseMenuPanel == null) return;

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
        if (button == null) return;
        TempleAudio.RegisterButton(button);
        button.onClick.AddListener(action);
    }
}
