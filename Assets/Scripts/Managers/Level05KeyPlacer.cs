using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level05KeyPlacer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlaceKeyForScene(SceneManager.GetActiveScene().name);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlaceKeyForScene(scene.name);
    }

    private static void PlaceKeyForScene(string sceneName)
    {
        string itemId;
        string displayName;
        Color color;

        if (!GetKeyInfo(sceneName, out itemId, out displayName, out color))
        {
            return;
        }

        if (PersistentInventory.WasCollected(itemId))
        {
            return;
        }

        if (GameObject.Find("Level05ChamberKey_" + itemId) != null)
        {
            return;
        }

        Vector3 position = GetPlacementPosition(sceneName);
        GameObject key = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        key.name = "Level05ChamberKey_" + itemId;
        key.transform.position = position;
        key.transform.localScale = new Vector3(0.35f, 0.08f, 0.35f);
        key.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        Renderer renderer = key.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        Light light = key.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 4f;
        light.intensity = 1.5f;
        light.color = color;

        Level05QuestItem questItem = key.AddComponent<Level05QuestItem>();
        questItem.itemId = itemId;
        questItem.displayName = displayName;
        questItem.itemColor = color;
    }

    private static bool GetKeyInfo(string sceneName, out string itemId, out string displayName, out Color color)
    {
        switch (sceneName)
        {
            case "Level01":
            case "Level01_Entrance":
                itemId = PersistentInventory.EmeraldKey;
                displayName = "Emerald Chamber Key";
                color = new Color(0.2f, 0.9f, 0.35f);
                return true;
            case "Level02_Corridor":
                itemId = PersistentInventory.SapphireKey;
                displayName = "Sapphire Chamber Key";
                color = new Color(0.25f, 0.45f, 1f);
                return true;
            case "Level03_Hall":
                itemId = PersistentInventory.RubyKey;
                displayName = "Ruby Chamber Key";
                color = new Color(1f, 0.2f, 0.18f);
                return true;
            case "Level04_Vault":
                itemId = PersistentInventory.SunKey;
                displayName = "Sun Chamber Key";
                color = new Color(1f, 0.85f, 0.15f);
                return true;
            default:
                itemId = string.Empty;
                displayName = string.Empty;
                color = Color.white;
                return false;
        }
    }

    private static Vector3 GetPlacementPosition(string sceneName)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player.transform.position + player.transform.right * 3f + Vector3.up * 1.2f;
        }

        switch (sceneName)
        {
            case "Level01":
            case "Level01_Entrance":
                return new Vector3(2.5f, 1.2f, 2.5f);
            case "Level02_Corridor":
                return new Vector3(3f, 1.2f, 0f);
            case "Level03_Hall":
                return new Vector3(-3f, 1.2f, 0f);
            case "Level04_Vault":
                return new Vector3(0f, 1.2f, 3f);
            default:
                return Vector3.up;
        }
    }
}
