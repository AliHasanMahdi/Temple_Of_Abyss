using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level05ChamberBuilder
{
    private const string RootName = "Level05ChamberRuntime";
    public const string VersionMarkerName = "Level05Chamber_GroundedDoorsTorches_v5";
    private const string ResourcePath = "Level05Chamber/";

    private static Material wallMaterial;
    private static Material tileMaterial;
    private static Material pillarMaterial;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        BuildIfNeeded(SceneManager.GetActiveScene().name);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BuildIfNeeded(scene.name);
    }

    public static GameObject BuildIfNeeded(string sceneName, bool movePlayerToEntrance = true)
    {
        if (sceneName != "Level05_Chamber")
        {
            return null;
        }

        GameObject existingRoot = GameObject.Find(RootName);
        if (existingRoot != null)
        {
            if (existingRoot.transform.Find(VersionMarkerName) == null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(existingRoot);
                }
                else
                {
                    Object.DestroyImmediate(existingRoot);
                }
            }
            else
            {
                EnsurePlayer(movePlayerToEntrance);
                TuneSceneLighting();
                return existingRoot;
            }
        }

        LoadMaterials();

        GameObject root = new GameObject(RootName);
        GameObject versionMarker = new GameObject(VersionMarkerName);
        versionMarker.transform.SetParent(root.transform, false);

        BuildMainChamber(root.transform);
        BuildPuzzleRooms(root.transform);
        BuildTreasureRoom(root.transform);
        BuildCentralOfferings(root.transform);
        Decorate(root.transform);
        EnsurePlayer(movePlayerToEntrance);
        TuneSceneLighting();

        return root;
    }

    private static void LoadMaterials()
    {
        wallMaterial = Resources.Load<Material>(ResourcePath + "M_Wall");
        tileMaterial = Resources.Load<Material>(ResourcePath + "M_Tile");
        pillarMaterial = Resources.Load<Material>(ResourcePath + "M_Pillar_A");
    }

    private static void BuildMainChamber(Transform root)
    {
        CreateFloor("Main Chamber Floor", root, Vector3.zero, new Vector3(32f, 0.3f, 22f));
        CreateRoof("Main Chamber Roof", root, new Vector3(0f, 4.1f, 0f), new Vector3(32f, 0.35f, 22f));

        CreateWall("Main North Wall Left", root, new Vector3(-9.5f, 2f, 11f), new Vector3(13f, 4f, 0.55f));
        CreateWall("Main North Wall Right", root, new Vector3(9.5f, 2f, 11f), new Vector3(13f, 4f, 0.55f));
        CreateWall("Main South Wall Left", root, new Vector3(-10f, 2f, -11f), new Vector3(11f, 4f, 0.55f));
        CreateWall("Main South Wall Right", root, new Vector3(10f, 2f, -11f), new Vector3(11f, 4f, 0.55f));
        CreateArch(root, new Vector3(0f, 0f, -11f), Quaternion.identity, new Vector3(1.6f, 1.4f, 1.4f));
        CreateEntranceLanding(root);

        CreateWall("Main West North Segment", root, new Vector3(-16f, 2f, 8.5f), new Vector3(0.55f, 4f, 5f));
        CreateWall("Main West Middle Segment", root, new Vector3(-16f, 2f, 0f), new Vector3(0.55f, 4f, 4f));
        CreateWall("Main West South Segment", root, new Vector3(-16f, 2f, -8.5f), new Vector3(0.55f, 4f, 5f));
        CreateWall("Main East North Segment", root, new Vector3(16f, 2f, 8.5f), new Vector3(0.55f, 4f, 5f));
        CreateWall("Main East Middle Segment", root, new Vector3(16f, 2f, 0f), new Vector3(0.55f, 4f, 4f));
        CreateWall("Main East South Segment", root, new Vector3(16f, 2f, -8.5f), new Vector3(0.55f, 4f, 5f));
    }

    private static void CreateEntranceLanding(Transform root)
    {
        CreateFloor("Entrance Landing Floor", root, new Vector3(0f, 0f, -14f), new Vector3(8f, 0.3f, 6f));
        CreateRoof("Entrance Landing Roof", root, new Vector3(0f, 4.1f, -14f), new Vector3(8f, 0.35f, 6f));
        CreateWall("Entrance Landing West Wall", root, new Vector3(-4f, 2f, -14f), new Vector3(0.55f, 4f, 6f));
        CreateWall("Entrance Landing East Wall", root, new Vector3(4f, 2f, -14f), new Vector3(0.55f, 4f, 6f));
        CreateWall("Entrance Landing South Left", root, new Vector3(-2.75f, 2f, -17f), new Vector3(2.5f, 4f, 0.55f));
        CreateWall("Entrance Landing South Right", root, new Vector3(2.75f, 2f, -17f), new Vector3(2.5f, 4f, 0.55f));
        CreateArch(root, new Vector3(0f, 0f, -17f), Quaternion.identity, new Vector3(1.25f, 1.2f, 1.2f));
        CreateWallTorch(root, new Vector3(-2.8f, -0.2f, -16.7f), Quaternion.identity);
        CreateWallTorch(root, new Vector3(2.8f, -0.2f, -16.7f), Quaternion.identity);
    }

    private static void BuildPuzzleRooms(Transform root)
    {
        BuildPuzzleRoom(root, "Emerald Puzzle Room", new Vector3(-25f, 0f, 6f), true, PersistentInventory.EmeraldKey, PersistentInventory.EmeraldGem, "Emerald Gem", new Color(0.2f, 0.9f, 0.35f));
        BuildPuzzleRoom(root, "Sapphire Puzzle Room", new Vector3(-25f, 0f, -6f), true, PersistentInventory.SapphireKey, PersistentInventory.SapphireGem, "Sapphire Gem", new Color(0.25f, 0.45f, 1f));
        BuildPuzzleRoom(root, "Ruby Puzzle Room", new Vector3(25f, 0f, 6f), false, PersistentInventory.RubyKey, PersistentInventory.RubyGem, "Ruby Gem", new Color(1f, 0.2f, 0.18f));
        BuildPuzzleRoom(root, "Sun Puzzle Room", new Vector3(25f, 0f, -6f), false, PersistentInventory.SunKey, PersistentInventory.SunGem, "Sun Gem", new Color(1f, 0.85f, 0.15f));
    }

    private static void BuildPuzzleRoom(Transform root, string roomName, Vector3 center, bool leftSide, string keyId, string rewardId, string rewardName, Color color)
    {
        GameObject room = new GameObject(roomName);
        room.transform.SetParent(root, false);

        CreateFloor(roomName + " Floor", room.transform, center, new Vector3(10f, 0.3f, 8f));
        CreateRoof(roomName + " Roof", room.transform, center + new Vector3(0f, 4.1f, 0f), new Vector3(10f, 0.35f, 8f));

        float outerX = center.x + (leftSide ? -5f : 5f);
        float innerX = center.x + (leftSide ? 5f : -5f);
        CreateWall(roomName + " Outer Wall", room.transform, new Vector3(outerX, 2f, center.z), new Vector3(0.55f, 4f, 8f));
        CreateWall(roomName + " North Wall", room.transform, center + new Vector3(0f, 2f, 4f), new Vector3(10f, 4f, 0.55f));
        CreateWall(roomName + " South Wall", room.transform, center + new Vector3(0f, 2f, -4f), new Vector3(10f, 4f, 0.55f));
        CreateWall(roomName + " Inner Wall Top", room.transform, new Vector3(innerX, 2f, center.z + 2.8f), new Vector3(0.55f, 4f, 2.4f));
        CreateWall(roomName + " Inner Wall Bottom", room.transform, new Vector3(innerX, 2f, center.z - 2.8f), new Vector3(0.55f, 4f, 2.4f));

        float corridorX = leftSide ? -18.5f : 18.5f;
        CreateFloor(roomName + " Connected Walkway", root, new Vector3(corridorX, 0f, center.z), new Vector3(5f, 0.3f, 3.2f));
        CreateRoof(roomName + " Walkway Roof", root, new Vector3(corridorX, 4.1f, center.z), new Vector3(5f, 0.35f, 3.2f));
        CreateWall(roomName + " Walkway North Rail", root, new Vector3(corridorX, 1.2f, center.z + 1.6f), new Vector3(5f, 2.4f, 0.35f));
        CreateWall(roomName + " Walkway South Rail", root, new Vector3(corridorX, 1.2f, center.z - 1.6f), new Vector3(5f, 2.4f, 0.35f));

        Vector3 doorPosition = new Vector3(innerX + (leftSide ? 0.25f : -0.25f), -0.2f, center.z);
        GameObject door = CreateDoor(roomName + " Locked Door", root, doorPosition, leftSide ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.Euler(0f, -90f, 0f), color);
        Level05ChamberDoor chamberDoor = door.AddComponent<Level05ChamberDoor>();
        chamberDoor.requiredItemId = keyId;
        chamberDoor.doorName = roomName;

        GameObject puzzle = CreatePedestal(roomName + " Puzzle Pedestal", room.transform, center + new Vector3(0f, 0.7f, 0f), color);
        Level05PuzzlePedestal puzzlePedestal = puzzle.AddComponent<Level05PuzzlePedestal>();
        puzzlePedestal.rewardItemId = rewardId;
        puzzlePedestal.rewardName = rewardName;
        puzzlePedestal.puzzleName = roomName;
        puzzlePedestal.rewardColor = color;

        CreateWallTorch(room.transform, center + new Vector3(leftSide ? -4.7f : 4.7f, -0.2f, 2.6f), leftSide ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.Euler(0f, -90f, 0f));
        CreateWallTorch(room.transform, center + new Vector3(leftSide ? -4.7f : 4.7f, -0.2f, -2.6f), leftSide ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.Euler(0f, -90f, 0f));
    }

    private static void BuildTreasureRoom(Transform root)
    {
        Vector3 center = new Vector3(0f, 0f, 19f);
        CreateFloor("Treasure Room Floor", root, center, new Vector3(18f, 0.3f, 10f));
        CreateRoof("Treasure Room Roof", root, center + new Vector3(0f, 4.3f, 0f), new Vector3(18f, 0.4f, 10f));
        CreateWall("Treasure North Wall", root, center + new Vector3(0f, 2f, 5f), new Vector3(18f, 4f, 0.55f));
        CreateWall("Treasure West Wall", root, center + new Vector3(-9f, 2f, 0f), new Vector3(0.55f, 4f, 10f));
        CreateWall("Treasure East Wall", root, center + new Vector3(9f, 2f, 0f), new Vector3(0.55f, 4f, 10f));
        CreateWall("Treasure Entrance Left Wall", root, new Vector3(-5.5f, 2f, 14f), new Vector3(7f, 4f, 0.55f));
        CreateWall("Treasure Entrance Right Wall", root, new Vector3(5.5f, 2f, 14f), new Vector3(7f, 4f, 0.55f));
        CreateFloor("Treasure Connected Walkway", root, new Vector3(0f, 0f, 12.3f), new Vector3(5.5f, 0.3f, 3f));

        GameObject treasure = CreatePedestal("Golden Statue", root, center + new Vector3(0f, 1.1f, 1f), new Color(1f, 0.72f, 0.18f));
        treasure.transform.localScale = new Vector3(1.2f, 1.9f, 1.2f);
        Level05QuestItem treasureItem = treasure.AddComponent<Level05QuestItem>();
        treasureItem.itemId = PersistentInventory.GoldenStatue;
        treasureItem.displayName = "Golden Statue";
        treasureItem.itemColor = new Color(1f, 0.72f, 0.18f);
        treasureItem.finishGameOnCollect = true;
        treasure.SetActive(false);

        GameObject door = CreateDoor("Treasure Room Sealed Door", root, new Vector3(0f, -0.2f, 14f), Quaternion.identity, new Color(1f, 0.72f, 0.18f));
        Level05ChamberDoor treasureDoor = door.AddComponent<Level05ChamberDoor>();
        treasureDoor.treasureDoor = true;
        treasureDoor.doorName = "Treasure Room";
        treasureDoor.treasureObject = treasure;

        CreateWallTorch(root, new Vector3(-4f, -0.2f, 14.4f), Quaternion.Euler(0f, 180f, 0f));
        CreateWallTorch(root, new Vector3(4f, -0.2f, 14.4f), Quaternion.Euler(0f, 180f, 0f));
    }

    private static void BuildCentralOfferings(Transform root)
    {
        CreateOffering(root, "Emerald Offering", new Vector3(-5f, 0.7f, 3f), PersistentInventory.EmeraldGem, PersistentInventory.EmeraldPlaced, "Emerald Gem", new Color(0.2f, 0.9f, 0.35f));
        CreateOffering(root, "Sapphire Offering", new Vector3(5f, 0.7f, 3f), PersistentInventory.SapphireGem, PersistentInventory.SapphirePlaced, "Sapphire Gem", new Color(0.25f, 0.45f, 1f));
        CreateOffering(root, "Ruby Offering", new Vector3(-5f, 0.7f, -3.5f), PersistentInventory.RubyGem, PersistentInventory.RubyPlaced, "Ruby Gem", new Color(1f, 0.2f, 0.18f));
        CreateOffering(root, "Sun Offering", new Vector3(5f, 0.7f, -3.5f), PersistentInventory.SunGem, PersistentInventory.SunPlaced, "Sun Gem", new Color(1f, 0.85f, 0.15f));
    }

    private static void CreateOffering(Transform root, string name, Vector3 position, string gemId, string placedId, string displayName, Color color)
    {
        GameObject pedestal = CreatePedestal(name, root, position, color);
        Level05OfferingPedestal offering = pedestal.AddComponent<Level05OfferingPedestal>();
        offering.requiredGemId = gemId;
        offering.placedId = placedId;
        offering.displayName = displayName;
        offering.placedColor = color;
    }

    private static void Decorate(Transform root)
    {
        CreatePrefab("Pillar_A", root, new Vector3(-14.2f, 0f, 9.2f), Quaternion.identity, Vector3.one);
        CreatePrefab("Pillar_A", root, new Vector3(14.2f, 0f, 9.2f), Quaternion.identity, Vector3.one);
        CreatePrefab("Pillar_B", root, new Vector3(-14.2f, 0f, -9.2f), Quaternion.identity, Vector3.one);
        CreatePrefab("Pillar_B", root, new Vector3(14.2f, 0f, -9.2f), Quaternion.identity, Vector3.one);

        CreateWallTorch(root, new Vector3(-6f, -0.2f, 10.6f), Quaternion.Euler(0f, 180f, 0f));
        CreateWallTorch(root, new Vector3(6f, -0.2f, 10.6f), Quaternion.Euler(0f, 180f, 0f));
        CreateWallTorch(root, new Vector3(-3f, -0.2f, -10.6f), Quaternion.identity);
        CreateWallTorch(root, new Vector3(3f, -0.2f, -10.6f), Quaternion.identity);

        CreateLight(root, new Vector3(0f, 3.7f, 0f), Color.white, 16f, 0.35f);
        CreateLight(root, new Vector3(0f, 3.8f, 18f), Color.white, 12f, 0.35f);
    }

    private static GameObject CreateDoor(string name, Transform parent, Vector3 position, Quaternion rotation, Color color)
    {
        GameObject doorRoot = new GameObject(name);
        doorRoot.transform.SetParent(parent, false);
        doorRoot.transform.position = position;
        doorRoot.transform.rotation = rotation;

        BoxCollider collider = doorRoot.AddComponent<BoxCollider>();
        collider.size = new Vector3(3.2f, 3.4f, 0.55f);
        collider.center = new Vector3(0f, 1.5f, 0f);

        GameObject visual = Resources.Load<GameObject>(ResourcePath + "Door_Prefab_Closed");
        if (visual != null)
        {
            GameObject instance = Object.Instantiate(visual, doorRoot.transform);
            instance.name = "Door Visual";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            Rigidbody[] bodies = instance.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody body in bodies)
            {
                body.useGravity = false;
                body.isKinematic = true;
            }
        }
        else
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = "Door Visual";
            block.transform.SetParent(doorRoot.transform, false);
            block.transform.localScale = new Vector3(3.2f, 3.2f, 0.35f);
            ApplyMaterial(block, wallMaterial);
        }

        CreateDoorFrame(doorRoot.transform);
        CreateDoorGem(doorRoot.transform, color);
        return doorRoot;
    }

    private static void CreateDoorFrame(Transform parent)
    {
        CreateLocalWall("Door Left Jamb", parent, new Vector3(-1.85f, 1.45f, 0f), new Vector3(0.35f, 3.25f, 0.75f));
        CreateLocalWall("Door Right Jamb", parent, new Vector3(1.85f, 1.45f, 0f), new Vector3(0.35f, 3.25f, 0.75f));
        CreateLocalWall("Door Top Lintel", parent, new Vector3(0f, 3.05f, 0f), new Vector3(4f, 0.35f, 0.85f));
        CreateLocalWall("Door Threshold", parent, new Vector3(0f, 0.08f, 0f), new Vector3(4f, 0.22f, 0.9f));
    }

    private static void CreateLocalWall(string name, Transform parent, Vector3 localPosition, Vector3 localScale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent, false);
        wall.transform.localPosition = localPosition;
        wall.transform.localRotation = Quaternion.identity;
        wall.transform.localScale = localScale;
        ApplyMaterial(wall, wallMaterial);
    }

    private static void CreateDoorGem(Transform parent, Color color)
    {
        GameObject gem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gem.name = "Door Key Color";
        gem.transform.SetParent(parent, false);
        gem.transform.localPosition = new Vector3(0f, 1.2f, -0.45f);
        gem.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);
        Renderer renderer = gem.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        Light light = gem.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 1.6f;
        light.intensity = 0.18f;
        light.color = color;
    }

    private static GameObject CreatePedestal(string name, Transform parent, Vector3 position, Color color)
    {
        GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.name = name;
        pedestal.transform.SetParent(parent, false);
        pedestal.transform.position = position;
        pedestal.transform.localScale = new Vector3(0.9f, 0.38f, 0.9f);
        ApplyMaterial(pedestal, pillarMaterial);

        GameObject gem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gem.name = name + " Gem";
        gem.transform.SetParent(pedestal.transform, false);
        gem.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        gem.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        Renderer renderer = gem.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        Light light = gem.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 2f;
        light.intensity = 0.25f;
        light.color = color;
        return pedestal;
    }

    private static void CreateFloor(string name, Transform parent, Vector3 position, Vector3 scale)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = name;
        floor.transform.SetParent(parent, false);
        floor.transform.position = position;
        floor.transform.localScale = scale;
        ApplyMaterial(floor, tileMaterial);
    }

    private static void CreateRoof(string name, Transform parent, Vector3 position, Vector3 scale)
    {
        GameObject roof = CreateWall(name, parent, position, scale);
        roof.name = name;
    }

    private static GameObject CreateWall(string name, Transform parent, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent, false);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        ApplyMaterial(wall, wallMaterial);
        return wall;
    }

    private static void ApplyMaterial(GameObject obj, Material material)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void CreatePrefab(string prefabName, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject prefab = Resources.Load<GameObject>(ResourcePath + prefabName);
        if (prefab == null)
        {
            return;
        }

        GameObject instance = Object.Instantiate(prefab, position, rotation, parent);
        instance.name = prefabName;
        instance.transform.localScale = scale;
    }

    private static void CreateArch(Transform parent, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        CreatePrefab("Arch_A", parent, position, rotation, scale);
    }

    private static void CreateWallTorch(Transform parent, Vector3 position, Quaternion rotation)
    {
        CreatePrefab("Torch_A", parent, position, rotation, Vector3.one);
        CreateLight(parent, position + new Vector3(0f, 0.5f, 0f), Color.white, 10f, 0.25f);
    }

    private static void CreateLight(Transform parent, Vector3 position, Color color, float range, float intensity)
    {
        GameObject lightObject = new GameObject("Warm Torch Light");
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.position = position;

        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.range = range;
        light.intensity = intensity;
        light.shadows = LightShadows.Soft;
    }

    private static void EnsurePlayer(bool movePlayerToEntrance)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && !IsUsablePlayerRig(player))
        {
            PlayerMovement existingPlayerRig = Object.FindFirstObjectByType<PlayerMovement>();
            if (existingPlayerRig != null && IsUsablePlayerRig(existingPlayerRig.gameObject))
            {
                player = existingPlayerRig.gameObject;
            }
            else
            {
                DestroySceneObject(player);
                player = null;
            }
        }

        if (player == null)
        {
            GameObject playerPrefab = Resources.Load<GameObject>(ResourcePath + "Player");
            if (playerPrefab != null)
            {
                player = Object.Instantiate(playerPrefab);
                player.name = "Player";
            }
        }

        if (player == null)
        {
            return;
        }

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        player.transform.position = movePlayerToEntrance ? new Vector3(0f, 2f, -14f) : player.transform.position;
        player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        EnsurePlayerCamera(player);

        if (controller != null)
        {
            controller.enabled = true;
        }

        DisableStandaloneMainCameras(player.transform);
    }

    private static bool IsUsablePlayerRig(GameObject player)
    {
        return player != null
            && player.GetComponent<PlayerMovement>() != null
            && player.GetComponent<AN_HeroInteractive>() != null
            && player.GetComponentInChildren<Camera>(true) != null;
    }

    private static void EnsurePlayerCamera(GameObject player)
    {
        Camera camera = player.GetComponentInChildren<Camera>(true);
        if (camera == null)
        {
            return;
        }

        camera.gameObject.SetActive(true);
        camera.enabled = true;
        camera.tag = "MainCamera";

        AudioListener listener = camera.GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = true;
        }

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.playerCamera = camera.transform;
            movement.cam = camera;
        }
    }

    private static void DestroySceneObject(GameObject objectToDestroy)
    {
        if (objectToDestroy == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(objectToDestroy);
        }
        else
        {
            Object.DestroyImmediate(objectToDestroy);
        }
    }

    private static void DisableStandaloneMainCameras(Transform player)
    {
        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera camera in cameras)
        {
            if (camera == null || camera.transform.IsChildOf(player))
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

    private static void TuneSceneLighting()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in lights)
        {
            if (light == null || light.type != LightType.Directional)
            {
                continue;
            }

            light.intensity = 0.25f;
            light.color = new Color(1f, 0.78f, 0.55f);
            light.shadows = LightShadows.Soft;
        }
    }
}
