using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;

    void Awake()
    {
        // Keep this object alive across all scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        // Save current scene
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("SavedScene", currentScene);

        // Save a readable level name
        string levelName = GetLevelName(currentScene);
        PlayerPrefs.SetString("SavedLevelName", levelName);

        // Save score
        if (HUDManager.Instance != null)
            PlayerPrefs.SetInt("SavedScore", HUDManager.Instance.GetScore());

        PlayerPrefs.Save();

        Debug.Log("Game Saved at: " + currentScene);
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey("SavedScene");
        PlayerPrefs.DeleteKey("SavedLevelName");
        PlayerPrefs.DeleteKey("SavedScore");
        PlayerPrefs.Save();
    }

    public bool HasSave()
    {
        return PlayerPrefs.HasKey("SavedScene");
    }

    string GetLevelName(string sceneName)
    {
        switch (sceneName)
        {
            case "Level01_Entrance": return "Level 1 - Temple Entrance";
            case "Level02_Corridor": return "Level 2 - Torch Corridor";
            case "Level03_Hall": return "Level 3 - Puzzle Hall";
            case "Level04_Vault": return "Level 4 - Abyss Vault";
            case "Level05_Chamber": return "Level 5 - Sacred Chamber";
            default: return sceneName;
        }
    }
}
