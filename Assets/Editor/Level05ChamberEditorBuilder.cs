using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class Level05ChamberEditorBuilder
{
    private const string SceneName = "Level05_Chamber";
    private const string ScenePath = "Assets/Scenes/Level05_Chamber.unity";
    private const string LevelOneScenePath = "Assets/Scenes/Level01.unity";
    private const string LevelOnePlayerPrefabPath = "Assets/Prefabs/Player.prefab";
    private const string RootName = "Level05ChamberRuntime";
    private const string CopiedSystemsMarkerName = "Level05_UsesCopiedLevel01Systems";

    private static readonly string[] LevelOneSystemNames =
    {
        "HUDCanvas",
        "HUDManager",
        "EventSystem",
        "Directional Light (1)"
    };

    private static readonly string[] LevelFiveGeneratedSystemNames =
    {
        CopiedSystemsMarkerName,
        "HUDCanvas",
        "HUDManager",
        "EventSystem",
        "Player",
        "PlayerBody",
        "Main Camera",
        "Directional Light",
        "Directional Light (1)"
    };

    static Level05ChamberEditorBuilder()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.delayCall += BuildForActiveScene;
    }

    [MenuItem("Temple Of Abyss/Build Level 5 Chamber")]
    public static void BuildForActiveScene()
    {
        if (Application.isPlaying)
        {
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != SceneName)
        {
            return;
        }

        RebuildOutdatedLayoutIfNeeded();
        GameObject root = Level05ChamberBuilder.BuildIfNeeded(activeScene.name, false);
        bool copiedSystems = CopyLevelOneSystemsIntoLevel05(activeScene, false);
        if (root != null || copiedSystems)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
        }
    }

    [MenuItem("Temple Of Abyss/Rebuild Level 5 Chamber")]
    public static void RebuildForActiveScene()
    {
        if (Application.isPlaying)
        {
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != SceneName)
        {
            EditorUtility.DisplayDialog("Level 5 Chamber", "Open Level05_Chamber before rebuilding the chamber.", "OK");
            return;
        }

        GameObject existingRoot = GameObject.Find(RootName);
        if (existingRoot != null)
        {
            Object.DestroyImmediate(existingRoot);
        }

        GameObject root = Level05ChamberBuilder.BuildIfNeeded(activeScene.name, false);
        bool copiedSystems = CopyLevelOneSystemsIntoLevel05(activeScene, true);
        if (root != null)
        {
            Selection.activeGameObject = root;
        }

        if (root != null || copiedSystems)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
        }
    }

    [MenuItem("Temple Of Abyss/Copy Level 1 Systems To Level 5")]
    public static void CopyLevelOneSystemsMenu()
    {
        if (Application.isPlaying)
        {
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != SceneName)
        {
            EditorUtility.DisplayDialog("Level 5 Chamber", "Open Level05_Chamber before copying Level 1 systems.", "OK");
            return;
        }

        if (CopyLevelOneSystemsIntoLevel05(activeScene, true))
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            EditorUtility.DisplayDialog("Level 5 Chamber", "Copied the real Level 1 HUD, pause menu, event system, player prefab, and light into Level05_Chamber.", "OK");
        }
    }

    public static void RebuildLevel05FromBatch()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject existingRoot = GameObject.Find(RootName);
        if (existingRoot != null)
        {
            Object.DestroyImmediate(existingRoot);
        }

        GameObject root = Level05ChamberBuilder.BuildIfNeeded(SceneName, true);
        bool copiedSystems = CopyLevelOneSystemsIntoLevel05(scene, true);
        if (root != null || copiedSystems)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (scene.name == SceneName)
        {
            EditorApplication.delayCall += BuildForActiveScene;
        }
    }

    private static void RebuildOutdatedLayoutIfNeeded()
    {
        GameObject existingRoot = GameObject.Find(RootName);
        if (existingRoot == null)
        {
            return;
        }

        if (existingRoot.transform.Find(Level05ChamberBuilder.VersionMarkerName) != null)
        {
            return;
        }

        Object.DestroyImmediate(existingRoot);
    }

    private static bool CopyLevelOneSystemsIntoLevel05(Scene targetScene, bool force)
    {
        if (!targetScene.IsValid() || targetScene.name != SceneName)
        {
            return false;
        }

        if (!force && HasCopiedLevelOneSystems(targetScene))
        {
            return false;
        }

        DestroyLevelFiveGeneratedSystems(targetScene);

        Scene activeScene = SceneManager.GetActiveScene();
        Scene sourceScene = EditorSceneManager.OpenScene(LevelOneScenePath, OpenSceneMode.Additive);
        GameObject copiedHudCanvas = null;
        GameObject copiedHudManager = null;
        GameObject copiedPlayer = null;

        try
        {
            foreach (string systemName in LevelOneSystemNames)
            {
                GameObject copiedObject = CopySceneObject(sourceScene, targetScene, systemName);
                if (copiedObject == null)
                {
                    Debug.LogWarning("Level 5 Chamber could not find Level 1 object to copy: " + systemName);
                    continue;
                }

                if (systemName == "HUDCanvas")
                {
                    copiedHudCanvas = copiedObject;
                }
                else if (systemName == "HUDManager")
                {
                    copiedHudManager = copiedObject;
                }
            }
        }
        finally
        {
            EditorSceneManager.CloseScene(sourceScene, true);
            if (activeScene.IsValid())
            {
                SceneManager.SetActiveScene(targetScene);
            }
        }

        copiedPlayer = CreateLevelOnePlayer(targetScene);
        WireCopiedLevelOneSystems(copiedHudCanvas, copiedHudManager);
        PlaceCopiedPlayer(copiedPlayer);
        DisableStandaloneMainCameras(targetScene, copiedPlayer != null ? copiedPlayer.transform : null);
        AddCopiedSystemsMarker(targetScene);
        return true;
    }

    private static bool HasCopiedLevelOneSystems(Scene targetScene)
    {
        return FindSceneObject(targetScene, CopiedSystemsMarkerName) != null
            && FindSceneObject(targetScene, "HUDCanvas") != null
            && FindSceneObject(targetScene, "HUDManager") != null
            && FindSceneObject(targetScene, "EventSystem") != null
            && FindSceneObject(targetScene, "Player") != null
            && FindSceneObject(targetScene, "PlayerCamera") != null;
    }

    private static void DestroyLevelFiveGeneratedSystems(Scene targetScene)
    {
        foreach (string objectName in LevelFiveGeneratedSystemNames)
        {
            GameObject objectToDestroy = FindSceneObject(targetScene, objectName);
            while (objectToDestroy != null)
            {
                Object.DestroyImmediate(objectToDestroy);
                objectToDestroy = FindSceneObject(targetScene, objectName);
            }
        }
    }

    private static GameObject CopySceneObject(Scene sourceScene, Scene targetScene, string objectName)
    {
        GameObject sourceObject = FindSceneObject(sourceScene, objectName);
        if (sourceObject == null)
        {
            return null;
        }

        GameObject copiedObject = Object.Instantiate(sourceObject);
        copiedObject.name = sourceObject.name;
        SceneManager.MoveGameObjectToScene(copiedObject, targetScene);
        return copiedObject;
    }

    private static GameObject CreateLevelOnePlayer(Scene targetScene)
    {
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LevelOnePlayerPrefabPath);
        if (playerPrefab == null)
        {
            Debug.LogWarning("Level 5 Chamber could not load Level 1 player prefab: " + LevelOnePlayerPrefabPath);
            return null;
        }

        GameObject player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
        if (player == null)
        {
            Debug.LogWarning("Level 5 Chamber could not instantiate Level 1 player prefab.");
            return null;
        }

        player.name = "Player";
        SceneManager.MoveGameObjectToScene(player, targetScene);

        Camera playerCamera = player.GetComponentInChildren<Camera>(true);
        if (playerCamera != null)
        {
            playerCamera.enabled = true;
            playerCamera.tag = "MainCamera";

            AudioListener listener = playerCamera.GetComponent<AudioListener>();
            if (listener != null)
            {
                listener.enabled = true;
            }
        }

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null && playerCamera != null)
        {
            movement.playerCamera = playerCamera.transform;
            movement.cam = playerCamera;
        }

        return player;
    }

    private static void WireCopiedLevelOneSystems(GameObject hudCanvas, GameObject hudManagerObject)
    {
        if (hudCanvas == null)
        {
            return;
        }

        if (hudManagerObject != null)
        {
            HUDManager hudManager = hudManagerObject.GetComponent<HUDManager>();
            if (hudManager != null)
            {
                hudManager.healthBar = FindComponentInChildrenByName<Slider>(hudCanvas.transform, "HealthBar");
                hudManager.scoreText = FindComponentInChildrenByName<TMP_Text>(hudCanvas.transform, "ScoreText");
                hudManager.checkpointText = FindComponentInChildrenByName<TMP_Text>(hudCanvas.transform, "CheckpointText");
                hudManager.interactPromptText = FindComponentInChildrenByName<TMP_Text>(hudCanvas.transform, "InteractPromptText");
            }
        }

        PauseMenu pauseMenu = hudCanvas.GetComponentInChildren<PauseMenu>(true);
        if (pauseMenu == null)
        {
            return;
        }

        pauseMenu.pauseMenuPanel = FindGameObjectInChildrenByName(hudCanvas.transform, "PauseMenuPanel");
        pauseMenu.settingsPanel = FindGameObjectInChildrenByName(hudCanvas.transform, "SettingsPanel");
        pauseMenu.resumeButton = FindComponentInChildrenByName<Button>(hudCanvas.transform, "ResumeButton");
        pauseMenu.settingsButton = FindComponentInChildrenByName<Button>(hudCanvas.transform, "SettingsButton");
        pauseMenu.restartButton = FindComponentInChildrenByName<Button>(hudCanvas.transform, "RestartButton");
        pauseMenu.mainMenuButton = FindComponentInChildrenByName<Button>(hudCanvas.transform, "MainMenuButton");
    }

    private static void PlaceCopiedPlayer(GameObject player)
    {
        if (player == null)
        {
            return;
        }

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        player.transform.position = new Vector3(0f, 2f, -14f);
        player.transform.rotation = Quaternion.identity;

        Rigidbody body = player.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    private static void DisableStandaloneMainCameras(Scene targetScene, Transform player)
    {
        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera camera in cameras)
        {
            if (camera == null || camera.gameObject.scene != targetScene)
            {
                continue;
            }

            if (player != null && camera.transform.IsChildOf(player))
            {
                continue;
            }

            if (camera.CompareTag("MainCamera") || camera.name == "Main Camera")
            {
                camera.enabled = false;
                AudioListener listener = camera.GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = false;
                }
            }
        }
    }

    private static void AddCopiedSystemsMarker(Scene targetScene)
    {
        GameObject marker = new GameObject(CopiedSystemsMarkerName);
        GameObject root = FindSceneObject(targetScene, RootName);
        SceneManager.MoveGameObjectToScene(marker, targetScene);
        if (root != null)
        {
            marker.transform.SetParent(root.transform, false);
        }
    }

    private static GameObject FindSceneObject(Scene scene, string objectName)
    {
        if (!scene.IsValid())
        {
            return null;
        }

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            if (rootObject.name == objectName)
            {
                return rootObject;
            }

            Transform child = FindChildRecursive(rootObject.transform, objectName);
            if (child != null)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private static GameObject FindGameObjectInChildrenByName(Transform root, string objectName)
    {
        Transform child = FindChildRecursive(root, objectName);
        return child != null ? child.gameObject : null;
    }

    private static T FindComponentInChildrenByName<T>(Transform root, string objectName) where T : Component
    {
        Transform child = FindChildRecursive(root, objectName);
        return child != null ? child.GetComponent<T>() : null;
    }

    private static Transform FindChildRecursive(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }

        foreach (Transform child in root)
        {
            if (child.name == objectName)
            {
                return child;
            }

            Transform nestedChild = FindChildRecursive(child, objectName);
            if (nestedChild != null)
            {
                return nestedChild;
            }
        }

        return null;
    }
}
