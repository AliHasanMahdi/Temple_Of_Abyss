using System.Collections.Generic;
using UnityEngine;

public class Level04LeverSequenceWallUnlock : MonoBehaviour
{
    const float ReleasedKeyDropKick = 0.65f;
    const float ReleasedKeyDropOffset = 0.08f;
    const float ReleasedKeyOutwardOffset = 0.12f;

    [Header("Levers")]
    [SerializeField] AN_Button[] leverButtons;
    [SerializeField] bool autoFindLeversByName = true;
    [SerializeField] int requiredLeverCount = 5;
    [SerializeField] string[] fallbackLeverNames =
    {
        "Lever_Prefab (1)",
        "Lever_Prefab (2)",
        "Lever_Prefab (3)",
        "Lever_Prefab (4)",
        "Lever_Prefab"
    };

    [Header("Solved Result")]
    [SerializeField] Rigidbody wallRigidbody;
    [SerializeField] GameObject releasedDoorKeyObject;
    [SerializeField] bool releaseWallOnSolve = true;
    [SerializeField] bool enableGravity = true;
    [SerializeField] bool startKinematic = false;
    [SerializeField] bool releaseDoorKeyOnSolve = true;
    [SerializeField] bool detachReleasedKey = true;
    [SerializeField] bool releasedKeyTrigger = false;
    [SerializeField] bool releasedKeyUseGravity = true;
    [SerializeField] bool releasedKeyIsKinematic = false;
    [SerializeField] bool resetReleasedKeyVelocity = true;
    [SerializeField] CollisionDetectionMode releasedKeyCollisionMode = CollisionDetectionMode.ContinuousDynamic;

    [Header("Audio")]
    [SerializeField] AudioClip unlockSound;
    [SerializeField] float unlockVolume = 0.9f;

    [Header("Debug")]
    [SerializeField] bool printSequenceToConsole = true;
    [SerializeField] bool useCustomSequence;
    [SerializeField] bool[] customSequence;
    [SerializeField] bool[] generatedSequence;

    AN_Button[] levers;
    bool checkingEnabled = true;

    void Awake()
    {
        if (unlockSound == null)
            unlockSound = TempleAudio.LoadClip("TempleAudio/SFX/Open Door 13");

        ResolveLevers();
        GenerateSequence();
    }

    void OnEnable()
    {
        AN_Button.LeverInteractionTriggered += OnLeverInteractionTriggered;
    }

    void OnDisable()
    {
        AN_Button.LeverInteractionTriggered -= OnLeverInteractionTriggered;
    }

    void Update()
    {
        if (!checkingEnabled)
            return;

        TryCompleteSequence();
    }

    void ResolveLevers()
    {
        List<AN_Button> foundLevers = new List<AN_Button>();

        if (leverButtons != null && leverButtons.Length > 0)
        {
            foreach (AN_Button lever in leverButtons)
            {
                if (lever != null)
                    foundLevers.Add(lever);
            }
        }

        if (foundLevers.Count == 0 && autoFindLeversByName)
        {
            foreach (string leverName in fallbackLeverNames)
            {
                GameObject leverObject = GameObject.Find(leverName);
                if (leverObject == null)
                    continue;

                AN_Button lever = leverObject.GetComponent<AN_Button>();
                if (lever == null)
                    lever = leverObject.GetComponentInChildren<AN_Button>(true);

                if (lever != null)
                    foundLevers.Add(lever);
            }
        }

        if (foundLevers.Count >= 5)
            requiredLeverCount = 5;
        else
            requiredLeverCount = Mathf.Min(4, foundLevers.Count);

        levers = foundLevers.GetRange(0, requiredLeverCount).ToArray();
        leverButtons = levers;

        if (printSequenceToConsole)
            Debug.Log("[Level04LeverSequenceWallUnlock] Found levers: " + BuildLeverNamesDebugString());
    }

    void GenerateSequence()
    {
        if (requiredLeverCount <= 0)
        {
            generatedSequence = System.Array.Empty<bool>();
            checkingEnabled = false;
            return;
        }

        if (useCustomSequence && customSequence != null && customSequence.Length >= requiredLeverCount)
        {
            generatedSequence = new bool[requiredLeverCount];
            bool customHasTrue = false;
            for (int i = 0; i < requiredLeverCount; i++)
            {
                generatedSequence[i] = customSequence[i];
                customHasTrue |= generatedSequence[i];
            }

            if (!customHasTrue)
                generatedSequence[0] = true;

            if (printSequenceToConsole)
                Debug.Log("[Level04LeverSequenceWallUnlock] Sequence: " + BuildSequenceDebugString());
            return;
        }

        generatedSequence = new bool[requiredLeverCount];
        bool anyTrue = false;

        for (int i = 0; i < generatedSequence.Length; i++)
        {
            generatedSequence[i] = Random.value >= 0.5f;
            anyTrue |= generatedSequence[i];
        }

        if (!anyTrue)
            generatedSequence[Random.Range(0, generatedSequence.Length)] = true;

        if (printSequenceToConsole)
            Debug.Log("[Level04LeverSequenceWallUnlock] Sequence: " + BuildSequenceDebugString());
    }

    void OnLeverInteractionTriggered(AN_Button lever)
    {
        if (!checkingEnabled || lever == null || levers == null)
            return;

        for (int i = 0; i < levers.Length; i++)
        {
            if (!ReferenceEquals(levers[i], lever))
                continue;

            if (printSequenceToConsole)
                Debug.Log("[Level04LeverSequenceWallUnlock] Lever toggled: " + lever.transform.root.name + " -> " + lever.IsPressed);

            TryCompleteSequence();
            return;
        }
    }

    void TryCompleteSequence()
    {
        if (!checkingEnabled)
            return;

        if (levers == null || generatedSequence == null || levers.Length == 0 || generatedSequence.Length == 0)
            return;

        if (levers.Length != generatedSequence.Length)
            return;

        for (int i = 0; i < generatedSequence.Length; i++)
        {
            if (levers[i] == null || levers[i].IsPressed != generatedSequence[i])
                return;
        }

        checkingEnabled = false;
        Debug.Log("sequence found");

        if (releaseWallOnSolve)
            ReleaseWall();

        if (releaseDoorKeyOnSolve)
            ReleaseDoorKey();

        if (unlockSound != null)
            TempleAudio.PlaySfx(unlockSound, unlockVolume);
    }

    void ReleaseWall()
    {
        if (wallRigidbody == null)
            wallRigidbody = GetComponent<Rigidbody>();

        if (wallRigidbody == null)
            return;

        wallRigidbody.isKinematic = startKinematic;
        wallRigidbody.useGravity = enableGravity;
        wallRigidbody.WakeUp();
    }

    void ReleaseDoorKey()
    {
        Transform mountedKeyTransform = releasedDoorKeyObject != null ? releasedDoorKeyObject.transform : null;
        Transform sourceParent = mountedKeyTransform != null ? mountedKeyTransform.parent : null;
        GameObject releasedKey = CreateReleasedDoorKeyInstance();
        if (releasedKey == null)
            return;

        Transform keyTransform = releasedKey.transform;
        Vector3 outwardDirection = Vector3.zero;
        Collider[] sourceColliders = sourceParent != null
            ? sourceParent.GetComponentsInChildren<Collider>(true)
            : System.Array.Empty<Collider>();
        Collider[] keyColliders = releasedKey.GetComponentsInChildren<Collider>(true);
        if (sourceParent != null)
        {
            outwardDirection = keyTransform.position - sourceParent.position;
            outwardDirection = Vector3.ProjectOnPlane(outwardDirection, Vector3.up).normalized;
        }

        if (detachReleasedKey)
            keyTransform.SetParent(null, true);

        keyTransform.position += Vector3.down * ReleasedKeyDropOffset;
        if (outwardDirection.sqrMagnitude > 0.0001f)
            keyTransform.position += outwardDirection * ReleasedKeyOutwardOffset;

        Collider collider = releasedKey.GetComponent<Collider>();
        if (collider != null)
            collider.isTrigger = releasedKeyTrigger;

        Rigidbody body = releasedKey.GetComponent<Rigidbody>();
        if (body == null)
            body = releasedKey.AddComponent<Rigidbody>();

        body.isKinematic = releasedKeyIsKinematic;
        body.useGravity = releasedKeyUseGravity;
        if (resetReleasedKeyVelocity)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        body.collisionDetectionMode = releasedKeyCollisionMode;
        Physics.SyncTransforms();
        body.WakeUp();
        IgnoreSourceDoorCollisions(keyColliders, sourceColliders);
        if (!body.isKinematic)
        {
            Vector3 releaseVelocity = Vector3.down;
            if (outwardDirection.sqrMagnitude > 0.0001f)
                releaseVelocity += outwardDirection * 0.45f;

            body.AddForce(releaseVelocity.normalized * ReleasedKeyDropKick, ForceMode.VelocityChange);
        }
    }

    GameObject CreateReleasedDoorKeyInstance()
    {
        if (releasedDoorKeyObject == null)
            return null;

        Transform mountedKeyTransform = releasedDoorKeyObject.transform;
        GameObject releasedKey = Instantiate(
            releasedDoorKeyObject,
            mountedKeyTransform.position,
            mountedKeyTransform.rotation);

        releasedKey.name = releasedDoorKeyObject.name;
        releasedKey.transform.localScale = mountedKeyTransform.lossyScale;
        DisableMountedDoorKey();
        return releasedKey;
    }

    void DisableMountedDoorKey()
    {
        if (releasedDoorKeyObject == null)
            return;

        Collider[] colliders = releasedDoorKeyObject.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
            collider.enabled = false;

        Renderer[] renderers = releasedDoorKeyObject.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
            renderer.enabled = false;

        releasedDoorKeyObject.SetActive(false);
    }

    static void IgnoreSourceDoorCollisions(Collider[] keyColliders, Collider[] sourceColliders)
    {
        if (keyColliders == null || sourceColliders == null)
            return;

        for (int i = 0; i < keyColliders.Length; i++)
        {
            Collider keyCollider = keyColliders[i];
            if (keyCollider == null)
                continue;

            for (int j = 0; j < sourceColliders.Length; j++)
            {
                Collider sourceCollider = sourceColliders[j];
                if (sourceCollider == null || sourceCollider == keyCollider)
                    continue;

                Physics.IgnoreCollision(keyCollider, sourceCollider, true);
            }
        }
    }

    string BuildSequenceDebugString()
    {
        if (generatedSequence == null || generatedSequence.Length == 0)
            return "(empty)";

        string[] parts = new string[generatedSequence.Length];
        for (int i = 0; i < generatedSequence.Length; i++)
            parts[i] = generatedSequence[i] ? "true" : "false";

        return string.Join(", ", parts);
    }

    string BuildLeverNamesDebugString()
    {
        if (levers == null || levers.Length == 0)
            return "(none)";

        string[] names = new string[levers.Length];
        for (int i = 0; i < levers.Length; i++)
            names[i] = levers[i] != null ? levers[i].transform.root.name : "(missing)";

        return string.Join(", ", names);
    }
}
