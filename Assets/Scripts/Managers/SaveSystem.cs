using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;

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
        }
    }

    public void SaveGame()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("SavedScene", currentScene);
        PlayerPrefs.SetString("SavedLevelName", GetLevelName(currentScene));

        // Save score
        if (HUDManager.Instance != null)
            PlayerPrefs.SetInt("SavedScore", HUDManager.Instance.GetScore());

        // Save player position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerPrefs.SetFloat("SavedPosX", player.transform.position.x);
            PlayerPrefs.SetFloat("SavedPosY", player.transform.position.y + 1f);
            PlayerPrefs.SetFloat("SavedPosZ", player.transform.position.z);
            Debug.Log("Saved position: " + player.transform.position);
        }

        PlayerPrefs.Save();
        Debug.Log("Game Saved at: " + currentScene);
    }

    public void SaveKeys()
    {
        AN_HeroInteractive hero = FindObjectOfType<AN_HeroInteractive>();
        if (hero == null) return;

        PlayerPrefs.SetInt("SavedRedKey", hero.RedKey ? 1 : 0);
        PlayerPrefs.SetInt("SavedBlueKey", hero.BlueKey ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("[SaveSystem] Keys saved — Red: " + hero.RedKey + "  Blue: " + hero.BlueKey);
    }

    public void LoadSavedPosition()
    {
        if (!HasSave()) return;

        string savedScene = PlayerPrefs.GetString("SavedScene", "");
        string currentScene = SceneManager.GetActiveScene().name;

        if (savedScene != currentScene) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found!");
            return;
        }

        float x = PlayerPrefs.GetFloat("SavedPosX", 0f);
        float y = PlayerPrefs.GetFloat("SavedPosY", 1f);
        float z = PlayerPrefs.GetFloat("SavedPosZ", 0f);

        Vector3 savedPos = new Vector3(x, y, z);

        // Disable CharacterController FIRST before moving
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Move player
        player.transform.position = savedPos;

        // Wait a frame then re-enable CharacterController
        StartCoroutine(ReEnableController(cc));

        // Restore score
        int savedScore = PlayerPrefs.GetInt("SavedScore", 0);
        if (HUDManager.Instance != null)
            HUDManager.Instance.SetScore(savedScore);

        // Restore keys
        AN_HeroInteractive hero = player.GetComponent<AN_HeroInteractive>();
        if (hero != null)
        {
            hero.RedKey = PlayerPrefs.GetInt("SavedRedKey", 0) == 1;
            hero.BlueKey = PlayerPrefs.GetInt("SavedBlueKey", 0) == 1;
            Debug.Log("[SaveSystem] Restored keys — Red: " + hero.RedKey + "  Blue: " + hero.BlueKey);
        }

        Debug.Log("Restored position to: " + savedPos);
    }

    IEnumerator ReEnableController(CharacterController cc)
    {
        yield return null; // wait one frame
        if (cc != null) cc.enabled = true;
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey("SavedScene");
        PlayerPrefs.DeleteKey("SavedLevelName");
        PlayerPrefs.DeleteKey("SavedScore");
        PlayerPrefs.DeleteKey("SavedPosX");
        PlayerPrefs.DeleteKey("SavedPosY");
        PlayerPrefs.DeleteKey("SavedPosZ");
        PlayerPrefs.DeleteKey("SavedRedKey");
        PlayerPrefs.DeleteKey("SavedBlueKey");
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