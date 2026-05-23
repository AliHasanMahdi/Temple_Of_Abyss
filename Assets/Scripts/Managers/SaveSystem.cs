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
            Vector3 safePosition = GetSafePlayerPosition(player.transform.position, player);
            PlayerPrefs.SetFloat("SavedPosX", safePosition.x);
            PlayerPrefs.SetFloat("SavedPosY", safePosition.y);
            PlayerPrefs.SetFloat("SavedPosZ", safePosition.z);
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

        RestorePlayerToSavedPosition(player);

        if (HUDManager.Instance != null)
            HUDManager.Instance.SetScore(PlayerPrefs.GetInt("SavedScore", 0));

        AN_HeroInteractive hero = player.GetComponent<AN_HeroInteractive>();
        if (hero != null)
        {
            hero.RedKey = PlayerPrefs.GetInt("SavedRedKey", 0) == 1;
            hero.BlueKey = PlayerPrefs.GetInt("SavedBlueKey", 0) == 1;
            Debug.Log("[SaveSystem] Restored keys — Red: " + hero.RedKey + "  Blue: " + hero.BlueKey);
        }

        Debug.Log("[SaveSystem] Restored position to: " + player.transform.position);
    }

    public void RestorePlayerToSavedPosition(GameObject player)
    {
        if (player == null || !PlayerPrefs.HasKey("SavedPosX")) return;

        Vector3 savedPosition = new Vector3(
            PlayerPrefs.GetFloat("SavedPosX", 0f),
            PlayerPrefs.GetFloat("SavedPosY", 1f),
            PlayerPrefs.GetFloat("SavedPosZ", 0f));

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = GetSafePlayerPosition(savedPosition, player);
        StartCoroutine(ReEnableController(cc, player));
    }

    IEnumerator ReEnableController(CharacterController cc, GameObject player)
    {
        yield return new WaitForFixedUpdate();
        yield return null;
        if (cc != null)
        {
            player.transform.position = GetSafePlayerPosition(player.transform.position, player);
            cc.enabled = true;
        }
    }

    public static Vector3 GetSafePlayerPosition(Vector3 position, GameObject player)
    {
        CharacterController cc = player != null ? player.GetComponent<CharacterController>() : null;
        float halfHeight = cc != null ? cc.height * 0.5f : 1f;
        float skin = cc != null ? Mathf.Max(cc.skinWidth, 0.05f) : 0.1f;
        int mask = Physics.DefaultRaycastLayers;

        Transform playerTransform = player != null ? player.transform : null;
        Vector3 rayStart = position + Vector3.up * 4f;
        RaycastHit[] hits = Physics.RaycastAll(rayStart, Vector3.down, 12f, mask, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null) continue;
            if (playerTransform != null && (hit.transform == playerTransform || hit.transform.IsChildOf(playerTransform))) continue;
            return hit.point + Vector3.up * (halfHeight + skin);
        }

        return position + Vector3.up * skin;
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

        // Clear level progress flags so New Game really starts fresh.
        PlayerPrefs.DeleteKey("KeyPickedUp_Key_01");
        PlayerPrefs.DeleteKey("KeyPickedUp_Key_02");
        PlayerPrefs.DeleteKey("Door_Door_01");
        PlayerPrefs.DeleteKey("Door_Door_02");
        PlayerPrefs.DeleteKey("Door_Door_03");
        PlayerPrefs.DeleteKey("CoinsCollected");
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
            case "Level01":
            case "Level01_Entrance":
                return "Level 1 - Temple Entrance";
            case "Level02_Corridor": return "Level 2 - Torch Corridor";
            case "Level03_Hall": return "Level 3 - Puzzle Hall";
            case "Level04_Vault": return "Level 4 - Abyss Vault";
            case "Level05_Chamber": return "Level 5 - Sacred Chamber";
            default: return sceneName;
        }
    }
}
