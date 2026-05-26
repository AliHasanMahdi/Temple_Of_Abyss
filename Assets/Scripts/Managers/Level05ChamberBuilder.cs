using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class Level05ChamberBuilder
{
    private const string RootName = "Level05ChamberRuntime";
    public const string VersionMarkerName = "Level05Chamber_Level01MainHall_v19";
    private const float FloorHeight = -0.2f;
    private const float TorchFlankOffset = 2.05f;
    private const float MainWallHeight = 5.4f;
    private const float MainWallCenterY = MainWallHeight * 0.5f;
    private const float MainCeilingY = MainWallHeight + 0.12f;
    private const float PuzzleRoomThresholdDepth = 8f;
    private const float PuzzleRoomThresholdOverlap = 1.1f;
    private const string PrefabFolder = "Assets/Prefabs/";
    private const string MaterialFolder = "Assets/Materials/";
    private const string ResourcePath = "Level05Chamber/";
    private const float ModularTileSize = 4f;
    private const float ModularWallHeight = 4f;

    private static Material wallMaterial;
    private static Material tileMaterial;
    private static Material pillarMaterial;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        HandleSceneLoaded(SceneManager.GetActiveScene().name);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleSceneLoaded(scene.name);
    }

    private static void HandleSceneLoaded(string sceneName)
    {
        if (sceneName != "Level05_Chamber")
        {
            return;
        }

        GameObject existingRoot = GameObject.Find(RootName);
        if (existingRoot != null && existingRoot.transform.Find(VersionMarkerName) != null)
        {
            SetGeneratedRoofsActive(existingRoot.transform, true);
            EnsurePlayer(true);
            ApplyLevelOneAtmosphere();
            return;
        }

        BuildIfNeeded(sceneName);
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
                SetGeneratedRoofsActive(existingRoot.transform, Application.isPlaying);
                EnsurePlayer(movePlayerToEntrance);
                ApplyLevelOneAtmosphere();
                return existingRoot;
            }
        }

        LoadMaterials();

        GameObject root = new GameObject(RootName);
        GameObject versionMarker = new GameObject(VersionMarkerName);
        versionMarker.transform.SetParent(root.transform, false);

        SetGeneratedRoofsActive(root.transform, Application.isPlaying);
        EnsurePlayer(movePlayerToEntrance);
        ApplyLevelOneAtmosphere();

        return root;
    }

    private static void LoadMaterials()
    {
        wallMaterial = LoadChamberMaterial("M_Wall");
        tileMaterial = LoadChamberMaterial("M_Tile");
        pillarMaterial = LoadChamberMaterial("M_Pillar_A");
    }

    private static GameObject LoadChamberPrefab(string prefabName)
    {
#if UNITY_EDITOR
        GameObject editorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + prefabName + ".prefab");
        if (editorPrefab != null)
        {
            return editorPrefab;
        }
#endif
        return Resources.Load<GameObject>(ResourcePath + prefabName);
    }

    private static Material LoadChamberMaterial(string materialName)
    {
#if UNITY_EDITOR
        Material editorMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + materialName + ".mat");
        if (editorMaterial != null)
        {
            return editorMaterial;
        }
#endif
        return Resources.Load<Material>(ResourcePath + materialName);
    }

    private static void EnsureInstanceMaterials(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    continue;
                }

                materials[i] = wallMaterial != null ? wallMaterial : tileMaterial;
                changed = true;
            }

            if (changed)
            {
                renderer.sharedMaterials = materials;
            }
        }
    }

    private static void BuildMainChamber(Transform root)
    {
        CreateTiledFloor("Main Chamber Floor", root, Vector3.zero, 32f, 22f);
        CreateCeiling("Main Chamber Roof", root, new Vector3(0f, MainCeilingY, 0f), 32f, 22f);

        CreateWall("Main North Wall Left", root, new Vector3(-9.5f, MainWallCenterY, 11f), new Vector3(13f, MainWallHeight, 0.55f));
        CreateWall("Main North Wall Right", root, new Vector3(9.5f, MainWallCenterY, 11f), new Vector3(13f, MainWallHeight, 0.55f));
        CreateWall("Main South Wall Left", root, new Vector3(-10f, MainWallCenterY, -11f), new Vector3(11f, MainWallHeight, 0.55f));
        CreateWall("Main South Wall Right", root, new Vector3(10f, MainWallCenterY, -11f), new Vector3(11f, MainWallHeight, 0.55f));
        CreateArchDoorway(root, "Main South Arch", new Vector3(0f, 0f, -11f), 0f, new Vector3(1.6f, 1.4f, 1.4f), null, false);
        CreateEntranceLanding(root);
        CreateMainSideWallSegments(root, "West", -16f);
        CreateMainSideWallSegments(root, "East", 16f);
        CreateMainRitualFloor(root);
    }

    private static void CreateMainSideWallSegments(Transform root, string sideName, float x)
    {
        // The side doors are 3.4 units wide. These segments close the shell tightly around the passages.
        CreateWall("Main " + sideName + " North Wall", root, new Vector3(x, MainWallCenterY, 9.35f), new Vector3(0.55f, MainWallHeight, 3.3f));
        CreateWall("Main " + sideName + " Center Wall", root, new Vector3(x, MainWallCenterY, 0f), new Vector3(0.55f, MainWallHeight, 8.6f));
        CreateWall("Main " + sideName + " South Wall", root, new Vector3(x, MainWallCenterY, -9.35f), new Vector3(0.55f, MainWallHeight, 3.3f));
    }

    private static void CreateMainRitualFloor(Transform root)
    {
        CreateTiledFloor("Main Ritual Floor", root, new Vector3(0f, 0.12f, -0.2f), 13f, 11f);
        CreateWall("Ritual Floor North Edge", root, new Vector3(0f, 0.1f, 5.45f), new Vector3(13.6f, 0.2f, 0.35f));
        CreateWall("Ritual Floor South Edge", root, new Vector3(0f, 0.1f, -5.85f), new Vector3(13.6f, 0.2f, 0.35f));
        CreateWall("Ritual Floor West Edge", root, new Vector3(-6.65f, 0.1f, -0.2f), new Vector3(0.35f, 0.2f, 11.6f));
        CreateWall("Ritual Floor East Edge", root, new Vector3(6.65f, 0.1f, -0.2f), new Vector3(0.35f, 0.2f, 11.6f));
    }

    private static void CreateEntranceLanding(Transform root)
    {
        CreateTiledFloor("Entrance Landing Floor", root, new Vector3(0f, 0f, -14f), 8f, 6f);
        CreateCeiling("Entrance Landing Roof", root, new Vector3(0f, 4.1f, -14f), 8f, 6f);
        CreateWall("Entrance Landing West Wall", root, new Vector3(-4f, 2f, -14f), new Vector3(0.55f, 4f, 6f));
        CreateWall("Entrance Landing East Wall", root, new Vector3(4f, 2f, -14f), new Vector3(0.55f, 4f, 6f));
        CreateWall("Entrance Landing South Left", root, new Vector3(-2.75f, 2f, -17f), new Vector3(2.5f, 4f, 0.55f));
        CreateWall("Entrance Landing South Right", root, new Vector3(2.75f, 2f, -17f), new Vector3(2.5f, 4f, 0.55f));
        CreatePrefab("Wall_Entrance", root, new Vector3(0f, FloorHeight, -17f), Quaternion.identity, Vector3.one);
        CreateWallTorch(root, new Vector3(-2.5f, FloorHeight, -16.8f), Quaternion.identity);
        CreateWallTorch(root, new Vector3(2.5f, FloorHeight, -16.8f), Quaternion.identity);
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

        float chamberEdgeX = leftSide ? -16f : 16f;
        float outerX = center.x + (leftSide ? -5f : 5f);
        float innerX = center.x + (leftSide ? 5f : -5f);
        float foundationWidth = Mathf.Abs(chamberEdgeX - outerX);
        Vector3 foundationCenter = new Vector3((outerX + chamberEdgeX) * 0.5f, 0f, center.z);

        // The room foundation reaches the main chamber edge; the doorway threshold below overlaps it.
        CreateTiledFloor(roomName + " Floor", room.transform, foundationCenter, foundationWidth, 8f);
        CreateCeiling(roomName + " Roof", room.transform, foundationCenter + new Vector3(0f, 4.1f, 0f), foundationWidth, 8f);

        CreateWall(roomName + " Outer Wall", room.transform, new Vector3(outerX, 2f, center.z), new Vector3(0.55f, 4f, 8f));
        CreateWall(roomName + " North Wall", room.transform, foundationCenter + new Vector3(0f, 2f, 4f), new Vector3(foundationWidth, 4f, 0.55f));
        CreateWall(roomName + " South Wall", room.transform, foundationCenter + new Vector3(0f, 2f, -4f), new Vector3(foundationWidth, 4f, 0.55f));
        CreateWall(roomName + " Inner Wall Top", room.transform, new Vector3(innerX, 2f, center.z + 2.8f), new Vector3(0.55f, 4f, 2.4f));
        CreateWall(roomName + " Inner Wall Bottom", room.transform, new Vector3(innerX, 2f, center.z - 2.8f), new Vector3(0.55f, 4f, 2.4f));

        CreatePuzzleRoomPassage(root, roomName, center, leftSide, keyId, color);

        GameObject puzzle = CreatePedestal(roomName + " Puzzle Pedestal", room.transform, center + new Vector3(0f, 0.7f, 0f), color);
        Level05PuzzlePedestal puzzlePedestal = puzzle.AddComponent<Level05PuzzlePedestal>();
        puzzlePedestal.rewardItemId = rewardId;
        puzzlePedestal.rewardName = rewardName;
        puzzlePedestal.puzzleName = roomName;
        puzzlePedestal.rewardColor = color;

        CreateWallTorch(room.transform, new Vector3(center.x, FloorHeight, center.z + 3.75f), Quaternion.Euler(0f, 180f, 0f));
        CreateWallTorch(room.transform, new Vector3(center.x, FloorHeight, center.z - 3.75f), Quaternion.identity);
    }

    private static GameObject CreatePuzzleRoomPassage(Transform root, string roomName, Vector3 center, bool leftSide, string keyId, Color keyColor)
    {
        float mainPortalX = leftSide ? -15.85f : 15.85f;
        float roomPortalX = center.x + (leftSide ? 5f : -5f);
        float mainYaw = leftSide ? 90f : -90f;
        float roomDoorYaw = leftSide ? -90f : 90f;
        Vector3 archScale = new Vector3(1.15f, 1.15f, 1.15f);

        Vector3 mainPortal = new Vector3(mainPortalX, 0f, center.z);
        float chamberThresholdX = mainPortalX + (leftSide ? PuzzleRoomThresholdOverlap : -PuzzleRoomThresholdOverlap);
        Vector3 thresholdCenter = new Vector3((chamberThresholdX + roomPortalX) * 0.5f, 0.02f, center.z);
        float thresholdWidth = Mathf.Abs(roomPortalX - chamberThresholdX) + 0.6f;
        CreateTiledFloor(roomName + " Chamber Threshold Floor", root, thresholdCenter, thresholdWidth, PuzzleRoomThresholdDepth);
        CreateArchDoorway(root, roomName + " Main Portal", mainPortal, mainYaw, archScale, null, false);
        CreateWallTorch(root, new Vector3(mainPortalX, FloorHeight, center.z - TorchFlankOffset), Quaternion.Euler(0f, mainYaw, 0f));
        CreateWallTorch(root, new Vector3(mainPortalX, FloorHeight, center.z + TorchFlankOffset), Quaternion.Euler(0f, mainYaw, 0f));
        CreatePrefab(leftSide ? "Pillar_A" : "Pillar_B", root, new Vector3(mainPortalX, 0f, center.z - TorchFlankOffset), Quaternion.identity, new Vector3(0.72f, 1f, 0.72f));
        CreatePrefab(leftSide ? "Pillar_A" : "Pillar_B", root, new Vector3(mainPortalX, 0f, center.z + TorchFlankOffset), Quaternion.identity, new Vector3(0.72f, 1f, 0.72f));

        Vector3 roomPortal = new Vector3(roomPortalX, 0f, center.z);
        GameObject door = CreateArchDoorway(root, roomName + " Locked Door", roomPortal, roomDoorYaw, archScale, keyColor, true);
        Level05ChamberDoor chamberDoor = door.AddComponent<Level05ChamberDoor>();
        chamberDoor.requiredItemId = keyId;
        chamberDoor.doorName = roomName;
        CreateWallTorch(root, new Vector3(roomPortalX, FloorHeight, center.z - TorchFlankOffset), Quaternion.Euler(0f, roomDoorYaw, 0f));
        CreateWallTorch(root, new Vector3(roomPortalX, FloorHeight, center.z + TorchFlankOffset), Quaternion.Euler(0f, roomDoorYaw, 0f));
        return door;
    }

    private static void BuildTreasureRoom(Transform root)
    {
        Vector3 center = new Vector3(0f, 0f, 19f);
        CreateTiledFloor("Treasure Room Floor", root, center, 18f, 10f);
        CreateCeiling("Treasure Room Roof", root, center + new Vector3(0f, 4.3f, 0f), 18f, 10f);
        CreateWall("Treasure North Wall", root, center + new Vector3(0f, 2f, 5f), new Vector3(18f, 4f, 0.55f));
        CreateWall("Treasure West Wall", root, center + new Vector3(-9f, 2f, 0f), new Vector3(0.55f, 4f, 10f));
        CreateWall("Treasure East Wall", root, center + new Vector3(9f, 2f, 0f), new Vector3(0.55f, 4f, 10f));
        CreateWall("Treasure Entrance Left Wall", root, new Vector3(-5.5f, 2f, 14f), new Vector3(7f, 4f, 0.55f));
        CreateWall("Treasure Entrance Right Wall", root, new Vector3(5.5f, 2f, 14f), new Vector3(7f, 4f, 0.55f));
        CreateTreasurePassage(root);
        DecorateTreasureRoom(root, center);
        CreateTreasureDais(root, center);

        GameObject treasure = CreatePedestal("Golden Statue", root, center + new Vector3(0f, 1.28f, 1.35f), new Color(1f, 0.72f, 0.18f));
        treasure.transform.localScale = new Vector3(1.2f, 1.9f, 1.2f);
        Level05QuestItem treasureItem = treasure.AddComponent<Level05QuestItem>();
        treasureItem.itemId = PersistentInventory.GoldenStatue;
        treasureItem.displayName = "Golden Statue";
        treasureItem.itemColor = new Color(1f, 0.72f, 0.18f);
        treasureItem.finishGameOnCollect = true;
        treasure.SetActive(false);

        GameObject door = CreateArchDoorway(root, "Treasure Room Sealed Door", new Vector3(0f, 0f, 14f), 0f, new Vector3(1.28f, 1.2f, 1.2f), new Color(1f, 0.72f, 0.18f), true);
        Level05ChamberDoor treasureDoor = door.AddComponent<Level05ChamberDoor>();
        treasureDoor.treasureDoor = true;
        treasureDoor.doorName = "Treasure Room";
        treasureDoor.treasureObject = treasure;

        CreateWallTorch(root, new Vector3(-3.5f, FloorHeight, 14.2f), Quaternion.Euler(0f, 180f, 0f));
        CreateWallTorch(root, new Vector3(3.5f, FloorHeight, 14.2f), Quaternion.Euler(0f, 180f, 0f));
    }

    private static void DecorateTreasureRoom(Transform root, Vector3 center)
    {
        for (float x = -7.5f; x <= 7.5f; x += 3.75f)
        {
            CreateWallTorch(root, center + new Vector3(x, FloorHeight, 4.6f), Quaternion.Euler(0f, 180f, 0f));
            CreatePrefab("Pillar_A", root, center + new Vector3(x, 0f, -3.8f), Quaternion.identity, new Vector3(0.75f, 1f, 0.75f));
        }

        CreatePrefab("Pillar_B", root, center + new Vector3(-6.5f, 0f, 2.5f), Quaternion.identity, new Vector3(0.7f, 1f, 0.7f));
        CreatePrefab("Pillar_B", root, center + new Vector3(6.5f, 0f, 2.5f), Quaternion.identity, new Vector3(0.7f, 1f, 0.7f));
    }

    private static void CreateTreasurePassage(Transform root)
    {
        CreateTiledFloor("Treasure Connected Passage Floor", root, new Vector3(0f, 0f, 12.45f), 5.7f, 3.45f);
        CreateCeiling("Treasure Connected Passage Roof", root, new Vector3(0f, 4.1f, 12.45f), 5.7f, 3.45f);
        CreateWall("Treasure Passage West Wall", root, new Vector3(-2.85f, 2f, 12.45f), new Vector3(0.45f, 4f, 3.45f));
        CreateWall("Treasure Passage East Wall", root, new Vector3(2.85f, 2f, 12.45f), new Vector3(0.45f, 4f, 3.45f));
        CreateArchDoorway(root, "Treasure Passage Arch", new Vector3(0f, 0f, 10.9f), 0f, new Vector3(1.28f, 1.2f, 1.2f), null, false);
        CreateWallTorch(root, new Vector3(-3.5f, FloorHeight, 10.9f), Quaternion.Euler(0f, 90f, 0f));
        CreateWallTorch(root, new Vector3(3.5f, FloorHeight, 10.9f), Quaternion.Euler(0f, -90f, 0f));
    }

    private static void CreateTreasureDais(Transform root, Vector3 center)
    {
        CreateTiledFloor("Treasure Statue Dais", root, center + new Vector3(0f, 0.18f, 1.3f), 8f, 4f);
        CreateWall("Treasure Dais Front Step", root, center + new Vector3(0f, 0.14f, -0.9f), new Vector3(8.4f, 0.28f, 0.65f));
        CreateWall("Treasure Dais West Edge", root, center + new Vector3(-4.15f, 0.18f, 1.3f), new Vector3(0.35f, 0.36f, 4.4f));
        CreateWall("Treasure Dais East Edge", root, center + new Vector3(4.15f, 0.18f, 1.3f), new Vector3(0.35f, 0.36f, 4.4f));
        CreateWall("Treasure Dais Back Edge", root, center + new Vector3(0f, 0.18f, 3.45f), new Vector3(8.4f, 0.36f, 0.35f));
    }

    private static void BuildCentralOfferings(Transform root)
    {
        // Sketch cross-pattern: gem color on pedestal matches the gem you place (not the room next to it).
        CreateOffering(root, "Sun Offering", new Vector3(-5f, 0.7f, 3f), PersistentInventory.SunGem, PersistentInventory.SunPlaced, "Sun Gem", new Color(1f, 0.85f, 0.15f));
        CreateOffering(root, "Sapphire Offering", new Vector3(5f, 0.7f, 3f), PersistentInventory.SapphireGem, PersistentInventory.SapphirePlaced, "Sapphire Gem", new Color(0.25f, 0.45f, 1f));
        CreateOffering(root, "Ruby Offering", new Vector3(-5f, 0.7f, -3.5f), PersistentInventory.RubyGem, PersistentInventory.RubyPlaced, "Ruby Gem", new Color(1f, 0.2f, 0.18f));
        CreateOffering(root, "Emerald Offering", new Vector3(5f, 0.7f, -3.5f), PersistentInventory.EmeraldGem, PersistentInventory.EmeraldPlaced, "Emerald Gem", new Color(0.2f, 0.9f, 0.35f));
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
        CreateMainChamberPillar(root, new Vector3(-14.2f, 0f, 9.2f), "Pillar_A");
        CreateMainChamberPillar(root, new Vector3(14.2f, 0f, 9.2f), "Pillar_A");
        CreateMainChamberPillar(root, new Vector3(-14.2f, 0f, -9.2f), "Pillar_B");
        CreateMainChamberPillar(root, new Vector3(14.2f, 0f, -9.2f), "Pillar_B");
        CreateMainChamberPillar(root, new Vector3(-8.6f, 0f, 0f), "Pillar_A");
        CreateMainChamberPillar(root, new Vector3(8.6f, 0f, 0f), "Pillar_B");
        CreatePrefab("Pillar_A", root, new Vector3(-7.6f, 0f, 20.8f), Quaternion.identity, new Vector3(0.8f, 1f, 0.8f));
        CreatePrefab("Pillar_A", root, new Vector3(7.6f, 0f, 20.8f), Quaternion.identity, new Vector3(0.8f, 1f, 0.8f));
        CreatePrefab("Pillar_B", root, new Vector3(-7.6f, 0f, 16.2f), Quaternion.identity, new Vector3(0.8f, 1f, 0.8f));
        CreatePrefab("Pillar_B", root, new Vector3(7.6f, 0f, 16.2f), Quaternion.identity, new Vector3(0.8f, 1f, 0.8f));

        CreateWallTorch(root, new Vector3(-15.6f, FloorHeight, 6f), Quaternion.Euler(0f, 90f, 0f));
        CreateWallTorch(root, new Vector3(-15.6f, FloorHeight, -6f), Quaternion.Euler(0f, 90f, 0f));
        CreateWallTorch(root, new Vector3(15.6f, FloorHeight, 6f), Quaternion.Euler(0f, -90f, 0f));
        CreateWallTorch(root, new Vector3(15.6f, FloorHeight, -6f), Quaternion.Euler(0f, -90f, 0f));
    }

    private static void CreateMainChamberPillar(Transform root, Vector3 position, string prefabName)
    {
        CreatePrefab(prefabName, root, position, Quaternion.identity, Vector3.one);
        CreateWall(prefabName + " Plinth", root, position + new Vector3(0f, 0.16f, 0f), new Vector3(1.55f, 0.32f, 1.55f));
    }

    private static GameObject CreateArchDoorway(Transform parent, string name, Vector3 archCenter, float yaw, Vector3 archScale, Color? keyColor, bool includeDoor)
    {
        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
        CreateArch(parent, archCenter, rotation, archScale);
        if (!includeDoor)
        {
            return null;
        }

        Vector3 doorPosition = new Vector3(archCenter.x, FloorHeight, archCenter.z);
        return CreateDoor(name, parent, doorPosition, rotation, keyColor ?? Color.clear, keyColor.HasValue);
    }

    private static GameObject CreateDoor(string name, Transform parent, Vector3 position, Quaternion rotation, Color keyColor, bool showKeyGem)
    {
        GameObject doorRoot = new GameObject(name);
        doorRoot.transform.SetParent(parent, false);
        doorRoot.transform.SetPositionAndRotation(position, rotation);

        BoxCollider collider = doorRoot.AddComponent<BoxCollider>();
        collider.size = new Vector3(2.8f, 3.2f, 0.6f);
        collider.center = new Vector3(0f, 1.6f, 0f);

        GameObject doorPrefab = LoadChamberPrefab("Door_Prefab_Closed");
        if (doorPrefab != null)
        {
            GameObject instance = Object.Instantiate(doorPrefab, doorRoot.transform);
            instance.name = "Door_Prefab_Closed";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            EnsureInstanceMaterials(instance);

            Rigidbody[] bodies = instance.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody body in bodies)
            {
                body.useGravity = false;
                body.isKinematic = true;
            }

            if (showKeyGem)
            {
                AttachDoorKeyGem(instance.transform, keyColor);
            }
        }
        else
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = "Door_Prefab_Closed";
            block.transform.SetParent(doorRoot.transform, false);
            block.transform.localPosition = Vector3.zero;
            block.transform.localScale = new Vector3(2.8f, 3.2f, 0.35f);
            ApplyMaterial(block, wallMaterial);
        }

        return doorRoot;
    }

    private static void AttachDoorKeyGem(Transform doorVisual, Color color)
    {
        GameObject gem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gem.name = "Door_key";
        gem.transform.SetParent(doorVisual, false);
        gem.transform.localPosition = new Vector3(0f, 1.35f, 0.05f);
        gem.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
        Renderer renderer = gem.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
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

        return pedestal;
    }

    private static void CreateTiledFloor(string name, Transform parent, Vector3 center, float sizeX, float sizeZ)
    {
        GameObject floorRoot = new GameObject(name);
        floorRoot.transform.SetParent(parent, false);
        floorRoot.transform.position = center;

        int tilesX = Mathf.Max(1, Mathf.CeilToInt(sizeX / ModularTileSize));
        int tilesZ = Mathf.Max(1, Mathf.CeilToInt(sizeZ / ModularTileSize));
        float tileSizeX = sizeX / tilesX;
        float tileSizeZ = sizeZ / tilesZ;
        float startX = -(sizeX * 0.5f) + tileSizeX * 0.5f;
        float startZ = -(sizeZ * 0.5f) + tileSizeZ * 0.5f;
        Vector3 tileScale = new Vector3(tileSizeX / ModularTileSize, 1f, tileSizeZ / ModularTileSize);

        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                Vector3 tilePosition = center + new Vector3(startX + x * tileSizeX, 0f, startZ + z * tileSizeZ);
                if (!TryCreatePrefab("Tile_A", floorRoot.transform, tilePosition, Quaternion.identity, tileScale))
                {
                    CreatePrimitiveTile(floorRoot.transform, tilePosition, tileSizeX, tileSizeZ);
                }
            }
        }
    }

    private static void CreatePrimitiveTile(Transform parent, Vector3 position, float sizeX, float sizeZ)
    {
        GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tile.name = "Tile_Fallback";
        tile.transform.SetParent(parent, false);
        tile.transform.position = position;
        tile.transform.localScale = new Vector3(sizeX, 0.3f, sizeZ);
        ApplyMaterial(tile, tileMaterial);
    }

    private static void CreateCeiling(string name, Transform parent, Vector3 center, float sizeX, float sizeZ)
    {
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = name;
        ceiling.transform.SetParent(parent, false);
        ceiling.transform.position = center;
        ceiling.transform.localScale = new Vector3(sizeX, 0.35f, sizeZ);
        ApplyMaterial(ceiling, wallMaterial);
        ceiling.SetActive(Application.isPlaying);
    }

    private static void SetGeneratedRoofsActive(Transform root, bool active)
    {
        if (root == null)
        {
            return;
        }

        foreach (Transform child in root)
        {
            if (child.name.Contains("Roof"))
            {
                child.gameObject.SetActive(active);
            }

            SetGeneratedRoofsActive(child, active);
        }
    }

    private static GameObject CreateWall(string name, Transform parent, Vector3 position, Vector3 scale)
    {
        GameObject modularWall = TryCreateModularWall(name, parent, position, scale);
        if (modularWall != null)
        {
            return modularWall;
        }

        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent, false);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        ApplyMaterial(wall, wallMaterial);
        return wall;
    }

    private static GameObject TryCreateModularWall(string name, Transform parent, Vector3 position, Vector3 scale)
    {
        GameObject wallPrefab = LoadChamberPrefab("Wall_A");
        if (wallPrefab == null)
        {
            return null;
        }

        bool alongX = scale.x >= scale.z;
        float length = alongX ? scale.x : scale.z;
        float height = scale.y;
        int segments = Mathf.Max(1, Mathf.CeilToInt(length / ModularTileSize));
        float segmentLength = length / segments;
        Quaternion rotation = alongX ? Quaternion.identity : Quaternion.Euler(0f, 90f, 0f);
        float yScale = height / ModularWallHeight;

        GameObject wallRoot = new GameObject(name);
        wallRoot.transform.SetParent(parent, false);
        // Layout calls use cube-style wall centers, while Wall_A is authored from its floor base.
        wallRoot.transform.position = position - new Vector3(0f, height * 0.5f, 0f);

        float start = -(length * 0.5f) + (segmentLength * 0.5f);
        for (int i = 0; i < segments; i++)
        {
            Vector3 offset = alongX
                ? new Vector3(start + i * segmentLength, 0f, 0f)
                : new Vector3(0f, 0f, start + i * segmentLength);

            GameObject segment = Object.Instantiate(wallPrefab, wallRoot.transform);
            segment.name = "Wall_A";
            segment.transform.localPosition = offset;
            segment.transform.localRotation = rotation;
            segment.transform.localScale = new Vector3(segmentLength / ModularTileSize, yScale, 1f);
            EnsureInstanceMaterials(segment);
        }

        return wallRoot;
    }

    private static void ApplyMaterial(GameObject obj, Material material)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static bool TryCreatePrefab(string prefabName, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject prefab = LoadChamberPrefab(prefabName);
        if (prefab == null)
        {
            return false;
        }

        GameObject instance = Object.Instantiate(prefab, position, rotation, parent);
        instance.name = prefabName;
        instance.transform.localScale = scale;
        EnsureInstanceMaterials(instance);
        return true;
    }

    private static void CreatePrefab(string prefabName, Transform parent, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (!TryCreatePrefab(prefabName, parent, position, rotation, scale))
        {
            Debug.LogWarning("Level 5 Chamber missing prefab: " + prefabName);
        }
    }

    private static void CreateArch(Transform parent, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        CreatePrefab("Arch_A", parent, position, rotation, scale);
    }

    private static void CreateWallTorch(Transform parent, Vector3 position, Quaternion rotation)
    {
        GameObject torchPrefab = LoadChamberPrefab("Torch_A");
        if (torchPrefab == null)
        {
            return;
        }

        GameObject torch = Object.Instantiate(torchPrefab, position, rotation, parent);
        torch.name = "Torch_A";
        torch.transform.localScale = Vector3.one;
        EnsureInstanceMaterials(torch);
        AttachTorchPointLight(torch.transform);
    }

    private static void AttachTorchPointLight(Transform torchTransform)
    {
        GameObject lightObject = new GameObject("Point Light");
        lightObject.transform.SetParent(torchTransform, false);
        lightObject.transform.localPosition = new Vector3(0f, 0.313f, 0.253f);
        lightObject.transform.localRotation = Quaternion.identity;

        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.66f, 0.36f);
        light.range = 8.5f;
        light.intensity = 0.34f;
        light.shadows = LightShadows.None;
    }

    private static void EnsurePlayer(bool movePlayerToEntrance)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        bool instantiatedPlayer = false;
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
            GameObject playerPrefab = LoadChamberPrefab("Player");
            if (playerPrefab != null)
            {
                player = Object.Instantiate(playerPrefab);
                player.name = "Player";
                instantiatedPlayer = true;
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

        if (movePlayerToEntrance && instantiatedPlayer)
        {
            player.transform.position = new Vector3(0f, 2f, -14f);
        }

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

    public static void ApplyLevelOneAtmosphere()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.0045f;
        RenderSettings.fogColor = new Color(0.35f, 0.34f, 0.32f, 1f);
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.212f, 0.227f, 0.259f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.114f, 0.125f, 0.133f, 1f);
        RenderSettings.ambientGroundColor = new Color(0.047f, 0.043f, 0.035f, 1f);
        RenderSettings.ambientIntensity = 1.15f;
        RenderSettings.skybox = null;

        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in lights)
        {
            if (light == null)
            {
                continue;
            }

            if (light.type == LightType.Directional)
            {
                light.intensity = 0.045f;
                light.color = new Color(0.92f, 0.9f, 0.82f, 1f);
                light.shadows = LightShadows.Soft;
            }
            else if (light.type == LightType.Point && light.transform.parent != null && light.transform.parent.name.Contains("Torch"))
            {
                light.intensity = 0.34f;
                light.range = 8.5f;
                light.color = new Color(1f, 0.66f, 0.36f);
                light.shadows = LightShadows.None;
            }
        }
    }
}
