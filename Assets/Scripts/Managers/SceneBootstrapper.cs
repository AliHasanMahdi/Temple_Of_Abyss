using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ConfigureScene(SceneManager.GetActiveScene().name);
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConfigureScene(scene.name);
    }

    static void ConfigureScene(string sceneName)
    {
        EnsureSaveSystemExists();

        if (!IsGameplayScene(sceneName))
            return;

        GameObject player = FindPlayer();
        if (player == null)
            return;

        if (sceneName == "Level04_Vault")
        {
            TuneLevel04Interaction(player);
        }
    }

    static void EnsureSaveSystemExists()
    {
        if (Object.FindFirstObjectByType<SaveSystem>() != null)
            return;

        new GameObject("SaveSystem").AddComponent<SaveSystem>();
    }

    static GameObject FindPlayer()
    {
        PlayerMovement playerMovement = Object.FindFirstObjectByType<PlayerMovement>();
        return playerMovement != null ? playerMovement.gameObject : null;
    }

    static void TuneLevel04Interaction(GameObject player)
    {
        PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
        if (interaction == null)
            return;

        interaction.interactionRange = 5f;
        interaction.interactionProbeRadius = 0.45f;
    }

    static bool IsGameplayScene(string sceneName)
    {
        return !string.IsNullOrEmpty(sceneName)
            && sceneName != "MainMenu"
            && sceneName != "GameOver";
    }

}
