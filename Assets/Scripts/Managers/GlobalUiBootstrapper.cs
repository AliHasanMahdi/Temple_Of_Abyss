using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalUiBootstrapper : MonoBehaviour
{
    static GlobalUiBootstrapper instance;
    static bool hasCanonicalUi;
    static bool isBootstrapping;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        if (instance != null)
            return;

        GameObject bootstrapper = new GameObject("GlobalUiBootstrapper");
        DontDestroyOnLoad(bootstrapper);
        instance = bootstrapper.AddComponent<GlobalUiBootstrapper>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(EnsureUiForScene(SceneManager.GetActiveScene()));
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
            StartCoroutine(EnsureUiForScene(scene));
    }

    IEnumerator EnsureUiForScene(Scene scene)
    {
        yield return null;

        if (!scene.IsValid() || SceneManager.GetActiveScene() != scene)
            yield break;

        if (scene.name == "MainMenu")
        {
            hasCanonicalUi = true;
            yield break;
        }

        if (scene.name == "GameOver")
            yield break;

        if (!hasCanonicalUi)
        {
            if (!isBootstrapping)
                yield return StartCoroutine(BootstrapCanonicalUi(scene));

            yield break;
        }

        CleanupSceneUi(scene);

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowHUD(scene.name != "MainMenu" && scene.name != "GameOver");
    }

    IEnumerator BootstrapCanonicalUi(Scene targetScene)
    {
        isBootstrapping = true;

        DestroyExistingUiInstances();

        AsyncOperation loadMainMenu = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
        if (loadMainMenu != null)
            yield return loadMainMenu;

        yield return null;

        hasCanonicalUi = HUDManager.Instance != null || PauseMenu.Instance != null || SettingsMenu.Instance != null;

        Scene mainMenuScene = SceneManager.GetSceneByName("MainMenu");
        if (mainMenuScene.IsValid() && mainMenuScene.isLoaded)
        {
            AsyncOperation unloadMainMenu = SceneManager.UnloadSceneAsync(mainMenuScene);
            if (unloadMainMenu != null)
                yield return unloadMainMenu;
        }

        CleanupSceneUi(targetScene);

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowHUD(targetScene.name != "MainMenu" && targetScene.name != "GameOver");

        isBootstrapping = false;
    }

    static void DestroyExistingUiInstances()
    {
        if (HUDManager.Instance != null)
        {
            Destroy(HUDManager.Instance.gameObject);
            HUDManager.Instance = null;
        }

        if (PauseMenu.Instance != null)
        {
            Destroy(PauseMenu.Instance.gameObject);
            PauseMenu.Instance = null;
        }

        if (SettingsMenu.Instance != null)
        {
            Destroy(SettingsMenu.Instance.gameObject);
            SettingsMenu.Instance = null;
        }
    }

    static void CleanupSceneUi(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return;

        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            if (root == null)
                continue;

            if (root.name == "HUDCanvas" ||
                root.name == "PauseMenuPanel" ||
                root.name == "HUDManager")
            {
                Destroy(root);
            }
        }
    }
}
