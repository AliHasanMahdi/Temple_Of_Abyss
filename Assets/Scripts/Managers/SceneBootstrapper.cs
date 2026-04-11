using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void ConfigureScene()
    {
        EnsureSaveSystemExists();

        if (SceneManager.GetActiveScene().name != "Level01_Entrance")
            return;

        EnsureGround();
        GameObject player = EnsurePlayer();
        EnsureHUDManager();
        TunePlayerController(player);
        EnsureSandboxGeometry();
        EnsureSandboxInteractables();
        DisableExtraCameras(player);
    }

    static void EnsureSaveSystemExists()
    {
        if (Object.FindFirstObjectByType<SaveSystem>() != null)
            return;

        new GameObject("SaveSystem").AddComponent<SaveSystem>();
    }

    static void EnsureGround()
    {
        GameObject ground = GameObject.Find("Plane");

        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Plane";
        }

        ground.transform.position = Vector3.zero;

        if (ground.transform.localScale.x < 8f || ground.transform.localScale.z < 8f)
            ground.transform.localScale = new Vector3(8f, 1f, 8f);
    }

    static GameObject EnsurePlayer()
    {
        PlayerMovement playerMovement = Object.FindFirstObjectByType<PlayerMovement>();

        if (playerMovement == null)
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1f, -2f);

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            controller.center = Vector3.zero;

            playerMovement = player.AddComponent<PlayerMovement>();
            player.AddComponent<PlayerHealth>();
            player.AddComponent<PlayerInteraction>();
            player.AddComponent<PlayerVisualAnimator>();
        }

        GameObject playerObject = playerMovement.gameObject;

        if (!playerObject.CompareTag("Player"))
            playerObject.tag = "Player";

        if (playerObject.transform.position.y < 0.5f)
            playerObject.transform.position = new Vector3(0f, 1f, -2f);

        if (playerMovement.mouseSensitivity < 40f)
            playerMovement.mouseSensitivity = 120f;

        if (playerMovement.playerCamera == null)
        {
            Transform cameraTransform = playerObject.transform.Find("PlayerCamera");

            if (cameraTransform == null)
            {
                GameObject cameraObject = new GameObject("PlayerCamera");
                cameraTransform = cameraObject.transform;
                cameraTransform.SetParent(playerObject.transform, false);
                cameraTransform.localPosition = new Vector3(0f, 0.75f, 0f);
            }

            playerMovement.playerCamera = cameraTransform;
        }

        if (playerMovement.cam == null)
        {
            Camera playerCamera = playerMovement.playerCamera.GetComponent<Camera>();
            if (playerCamera == null)
                playerCamera = playerMovement.playerCamera.gameObject.AddComponent<Camera>();

            playerMovement.cam = playerCamera;
        }

        if (playerMovement.playerCamera.GetComponent<AudioListener>() == null)
            playerMovement.playerCamera.gameObject.AddComponent<AudioListener>();

        if (playerObject.GetComponent<PlayerHealth>() == null)
            playerObject.AddComponent<PlayerHealth>();

        if (playerObject.GetComponent<PlayerInteraction>() == null)
            playerObject.AddComponent<PlayerInteraction>();

        if (playerObject.GetComponent<PlayerVisualAnimator>() == null)
            playerObject.AddComponent<PlayerVisualAnimator>();

        return playerObject;
    }

    static void EnsureHUDManager()
    {
        if (Object.FindFirstObjectByType<HUDManager>() != null)
            return;

        new GameObject("HUDManager").AddComponent<HUDManager>();
    }

    static void TunePlayerController(GameObject player)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.walkSpeed = 4.5f;
            movement.runSpeed = 7.5f;
            movement.jumpHeight = 1.8f;
            movement.gravity = -18f;
            movement.mouseSensitivity = 120f;
            movement.normalFOV = 65f;
            movement.sprintFOV = 78f;
            movement.crouchHeight = 1.1f;
        }

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 45f;
            controller.skinWidth = 0.08f;
            controller.minMoveDistance = 0f;
        }
    }

    static void EnsureSandboxGeometry()
    {
        GameObject root = FindOrCreate("SandboxGeometry");

        EnsureCube(root.transform, "NorthWall", new Vector3(0f, 2f, 38f), new Vector3(76f, 4f, 1f), Color.gray);
        EnsureCube(root.transform, "SouthWall", new Vector3(0f, 2f, -38f), new Vector3(76f, 4f, 1f), Color.gray);
        EnsureCube(root.transform, "WestWall", new Vector3(-38f, 2f, 0f), new Vector3(1f, 4f, 76f), Color.gray);
        EnsureCube(root.transform, "EastWall", new Vector3(38f, 2f, 0f), new Vector3(1f, 4f, 76f), Color.gray);

        EnsureCube(root.transform, "Ramp", new Vector3(-10f, 1f, 10f), new Vector3(6f, 1.5f, 12f), new Color(0.55f, 0.48f, 0.38f, 1f), new Vector3(24f, 0f, 0f));
        EnsureCube(root.transform, "UpperPlatform", new Vector3(-10f, 3.5f, 20f), new Vector3(12f, 1f, 12f), new Color(0.42f, 0.42f, 0.46f, 1f));
        EnsureCube(root.transform, "TestBlockA", new Vector3(8f, 1f, 6f), new Vector3(2f, 2f, 2f), new Color(0.4f, 0.5f, 0.55f, 1f));
        EnsureCube(root.transform, "TestBlockB", new Vector3(13f, 1.5f, 10f), new Vector3(3f, 3f, 3f), new Color(0.38f, 0.42f, 0.5f, 1f));
        EnsureCube(root.transform, "TestBlockC", new Vector3(18f, 0.75f, 5f), new Vector3(4f, 1.5f, 4f), new Color(0.46f, 0.42f, 0.34f, 1f));
    }

    static void EnsureSandboxInteractables()
    {
        GameObject root = FindOrCreate("SandboxInteractables");

        GameObject door = EnsureCube(root.transform, "StoneDoor", new Vector3(0f, 2f, 16f), new Vector3(3f, 4f, 0.75f), new Color(0.32f, 0.34f, 0.38f, 1f));
        DoorInteractable doorInteractable = EnsureComponent<DoorInteractable>(door);
        doorInteractable.displayName = "stone door";
        doorInteractable.promptVerb = "open";

        GameObject pickup = EnsureSphere(root.transform, "RelicPickup", new Vector3(8f, 1.2f, 4f), new Vector3(1.2f, 1.2f, 1.2f), new Color(0.85f, 0.72f, 0.22f, 1f));
        PickupInteractable pickupInteractable = EnsureComponent<PickupInteractable>(pickup);
        pickupInteractable.displayName = "temple relic";
        pickupInteractable.promptVerb = "collect";
        pickupInteractable.rewardType = PickupInteractable.RewardType.Score;
        pickupInteractable.amount = 25;

        GameObject checkpoint = EnsureCylinder(root.transform, "CheckpointShrine", new Vector3(-12f, 1.5f, 22f), new Vector3(1.5f, 1.5f, 1.5f), new Color(0.24f, 0.68f, 0.78f, 1f));
        CheckpointInteractable checkpointInteractable = EnsureComponent<CheckpointInteractable>(checkpoint);
        checkpointInteractable.displayName = "checkpoint shrine";
        checkpointInteractable.promptVerb = "activate";
    }

    static GameObject FindOrCreate(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null)
            target = new GameObject(objectName);

        return target;
    }

    static GameObject EnsureCube(Transform parent, string objectName, Vector3 position, Vector3 scale, Color color, Vector3? eulerAngles = null)
    {
        return EnsurePrimitive(parent, objectName, PrimitiveType.Cube, position, scale, color, eulerAngles);
    }

    static GameObject EnsureSphere(Transform parent, string objectName, Vector3 position, Vector3 scale, Color color)
    {
        return EnsurePrimitive(parent, objectName, PrimitiveType.Sphere, position, scale, color, null);
    }

    static GameObject EnsureCylinder(Transform parent, string objectName, Vector3 position, Vector3 scale, Color color)
    {
        return EnsurePrimitive(parent, objectName, PrimitiveType.Cylinder, position, scale, color, null);
    }

    static GameObject EnsurePrimitive(Transform parent, string objectName, PrimitiveType primitiveType, Vector3 position, Vector3 scale, Color color, Vector3? eulerAngles)
    {
        GameObject target = GameObject.Find(objectName);

        if (target == null)
        {
            target = GameObject.CreatePrimitive(primitiveType);
            target.name = objectName;
        }

        target.transform.SetParent(parent, true);
        target.transform.position = position;
        target.transform.localScale = scale;

        if (eulerAngles.HasValue)
            target.transform.rotation = Quaternion.Euler(eulerAngles.Value);

        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = color;

        return target;
    }

    static T EnsureComponent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if (component == null)
            component = target.AddComponent<T>();

        return component;
    }

    static void DisableExtraCameras(GameObject player)
    {
        Camera playerCamera = player.GetComponentInChildren<Camera>(true);
        Camera[] sceneCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);

        foreach (Camera camera in sceneCameras)
        {
            bool isPlayerCamera = camera == playerCamera;
            camera.enabled = isPlayerCamera;

            AudioListener listener = camera.GetComponent<AudioListener>();
            if (listener != null)
                listener.enabled = isPlayerCamera;

            if (isPlayerCamera)
                camera.tag = "MainCamera";
        }
    }
}
