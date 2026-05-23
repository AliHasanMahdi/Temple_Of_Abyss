using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;

    // ── IN-MEMORY STATE (not written to disk until checkpoint) ────
    // These are cleared automatically on scene reload (object is DontDestroyOnLoad
    // but the sets are re-initialised each time DeleteSave/LoadSavedPosition is called,
    // and more importantly the disk keys are NOT written until SaveGame() runs).
    private HashSet<string> _pendingUnlockedDoors = new HashSet<string>();
    private HashSet<string> _pendingPickedUpKeys = new HashSet<string>();
    // ─────────────────────────────────────────────────────────────

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

    // ── PENDING SETTERS (memory only, no disk write) ──────────────

    /// <summary>Called by AN_DoorKey when picked up — held in memory until checkpoint.</summary>
    public void PendingKeyPickup(string keyID, bool isRed)
    {
        _pendingPickedUpKeys.Add(keyID);
        Debug.Log("[SaveSystem] Key pickup pending checkpoint — " + keyID);
    }

    /// <summary>Called by AN_DoorScript / EnemyRoom when a door is unlocked — held in memory until checkpoint.</summary>
    public void PendingDoorUnlocked(string doorID)
    {
        _pendingUnlockedDoors.Add(doorID);
        Debug.Log("[SaveSystem] Door unlock pending checkpoint — " + doorID);
    }

    // ── STATE QUERIES ─────────────────────────────────────────────

    /// <summary>
    /// Returns true only if this key was picked up AND a checkpoint was reached after that.
    /// (Checks disk only — pending memory is irrelevant here because it is cleared on
    /// scene reload, so a key picked up before death will correctly respawn.)
    /// </summary>
    public bool IsKeyPickedUp(string keyID)
    {
        return PlayerPrefs.GetInt("KeyPickedUp_" + keyID, 0) == 1;
    }

    /// <summary>
    /// Returns true if a door was unlocked in a previous saved session (disk),
    /// OR was unlocked in the current session (memory) — so it stays open
    /// within the same run even before a checkpoint.
    /// </summary>
    public bool IsDoorUnlocked(string doorID)
    {
        return PlayerPrefs.GetInt("Door_" + doorID, 0) == 1
            || _pendingUnlockedDoors.Contains(doorID);
    }

    // ── CHECKPOINT SAVE (flushes everything to disk) ──────────────

    /// <summary>Called by Checkpoint — saves position, score, keys, and doors to disk.</summary>
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

            // Flush key inventory
            AN_HeroInteractive hero = player.GetComponent<AN_HeroInteractive>();
            if (hero != null)
            {
                PlayerPrefs.SetInt("SavedRedKey", hero.RedKey ? 1 : 0);
                PlayerPrefs.SetInt("SavedBlueKey", hero.BlueKey ? 1 : 0);
            }
        }

        // Flush pending door unlocks to disk
        foreach (string doorID in _pendingUnlockedDoors)
            PlayerPrefs.SetInt("Door_" + doorID, 1);

        // Flush pending key pickup flags to disk
        foreach (string keyID in _pendingPickedUpKeys)
            PlayerPrefs.SetInt("KeyPickedUp_" + keyID, 1);

        PlayerPrefs.Save();

        // Clear pending sets — now safely on disk
        _pendingUnlockedDoors.Clear();
        _pendingPickedUpKeys.Clear();

        Debug.Log("[SaveSystem] Game saved at checkpoint: " + currentScene);
    }

    // ── LOAD (called after respawn) ───────────────────────────────

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

        // Clear session memory on respawn — pending state is lost on death by design
        _pendingUnlockedDoors.Clear();
        _pendingPickedUpKeys.Clear();

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

    // ── DELETE SAVE ───────────────────────────────────────────────

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

        PlayerPrefs.DeleteKey("KeyPickedUp_Key_01");
        PlayerPrefs.DeleteKey("KeyPickedUp_Key_02");
        PlayerPrefs.DeleteKey("Door_Door_01");
        PlayerPrefs.DeleteKey("Door_Door_02");
        PlayerPrefs.DeleteKey("Door_Door_03");
        PlayerPrefs.DeleteKey("CoinsCollected");
        PlayerPrefs.Save();

        _pendingUnlockedDoors.Clear();
        _pendingPickedUpKeys.Clear();
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
            case "Level01_Entrance": return "Level 1 - Temple Entrance";
            case "Level02_Corridor": return "Level 2 - Torch Corridor";
            case "Level03_Hall": return "Level 3 - Puzzle Hall";
            case "Level04_Vault": return "Level 4 - Abyss Vault";
            case "Level05_Chamber": return "Level 5 - Sacred Chamber";
            default: return sceneName;
        }
    }
}