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

    // Called by Checkpoint — saves position, score, keys, and door states
    public void SaveGame()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("SavedScene", currentScene);
        PlayerPrefs.SetString("SavedLevelName", GetLevelName(currentScene));

        if (HUDManager.Instance != null)
            PlayerPrefs.SetInt("SavedScore", HUDManager.Instance.GetScore());

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerPrefs.SetFloat("SavedPosX", player.transform.position.x);
            PlayerPrefs.SetFloat("SavedPosY", player.transform.position.y + 1f);
            PlayerPrefs.SetFloat("SavedPosZ", player.transform.position.z);
        }

        SaveKeys();
        PlayerPrefs.Save();
        Debug.Log("[SaveSystem] Game saved at: " + currentScene);
    }

    // Called when player picks up a key — saves key state immediately
    public void SaveKeys()
    {
        AN_HeroInteractive hero = FindObjectOfType<AN_HeroInteractive>();
        if (hero == null) return;

        PlayerPrefs.SetInt("SavedRedKey", hero.RedKey ? 1 : 0);
        PlayerPrefs.SetInt("SavedBlueKey", hero.BlueKey ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("[SaveSystem] Keys saved — Red: " + hero.RedKey + "  Blue: " + hero.BlueKey);
    }

    // Called when a door is unlocked — saves that door's unlocked state
    public void SaveDoorUnlocked(string doorID)
    {
        PlayerPrefs.SetInt("Door_" + doorID, 1);
        PlayerPrefs.Save();
        Debug.Log("[SaveSystem] Door saved as unlocked: " + doorID);
    }

    // Check if a door was already unlocked before player died
    public bool IsDoorUnlocked(string doorID)
    {
        return PlayerPrefs.GetInt("Door_" + doorID, 0) == 1;
    }

    // Called after respawn — restores position, score, keys
    public void LoadSavedPosition()
    {
        if (!HasSave()) return;

        string savedScene = PlayerPrefs.GetString("SavedScene", "");
        string currentScene = SceneManager.GetActiveScene().name;
        if (savedScene != currentScene) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float x = PlayerPrefs.GetFloat("SavedPosX", 0f);
        float y = PlayerPrefs.GetFloat("SavedPosY", 1f);
        float z = PlayerPrefs.GetFloat("SavedPosZ", 0f);

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = new Vector3(x, y, z);
        StartCoroutine(ReEnableController(cc));

        if (HUDManager.Instance != null)
            HUDManager.Instance.SetScore(PlayerPrefs.GetInt("SavedScore", 0));

        AN_HeroInteractive hero = player.GetComponent<AN_HeroInteractive>();
        if (hero != null)
        {
            hero.RedKey = PlayerPrefs.GetInt("SavedRedKey", 0) == 1;
            hero.BlueKey = PlayerPrefs.GetInt("SavedBlueKey", 0) == 1;
            Debug.Log("[SaveSystem] Restored keys — Red: " + hero.RedKey + "  Blue: " + hero.BlueKey);
        }

        Debug.Log("[SaveSystem] Restored position to: " + x + ", " + y + ", " + z);
    }

    IEnumerator ReEnableController(CharacterController cc)
    {
        yield return null;
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