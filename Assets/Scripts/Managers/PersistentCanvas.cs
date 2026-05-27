using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentCanvas : MonoBehaviour
{
    public static PersistentCanvas Instance;

    [Header("Panels")]
    public GameObject hudCanvas;
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Hide pause and settings on every scene load
        if (PauseMenu.Instance != null)
            PauseMenu.Instance.Resume();
        else if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Hide HUD on MainMenu, show it in all game levels
        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowHUD(scene.name != "MainMenu");
        else if (hudCanvas != null)
            hudCanvas.SetActive(scene.name != "MainMenu");
    }
}
