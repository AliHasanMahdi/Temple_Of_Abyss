using System.Collections.Generic;
using UnityEngine;

public class Level04LeverSequenceWallUnlock : MonoBehaviour
{
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

    void Update()
    {
        if (!checkingEnabled || levers == null || generatedSequence == null)
            return;

        for (int i = 0; i < generatedSequence.Length; i++)
        {
            if (levers[i] == null || levers[i].IsPressed != generatedSequence[i])
                return;
        }

        checkingEnabled = false;

        if (releaseWallOnSolve)
            ReleaseWall();

        if (releaseDoorKeyOnSolve)
            ReleaseDoorKey();

        if (unlockSound != null)
            TempleAudio.PlaySfx(unlockSound, unlockVolume);

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
        if (releasedDoorKeyObject == null)
            return;

        if (detachReleasedKey)
            releasedDoorKeyObject.transform.SetParent(null, true);

        Collider collider = releasedDoorKeyObject.GetComponent<Collider>();
        if (collider != null)
            collider.isTrigger = releasedKeyTrigger;

        Rigidbody body = releasedDoorKeyObject.GetComponent<Rigidbody>();
        if (body == null)
            body = releasedDoorKeyObject.AddComponent<Rigidbody>();

        body.isKinematic = releasedKeyIsKinematic;
        body.useGravity = releasedKeyUseGravity;
        if (resetReleasedKeyVelocity)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        body.collisionDetectionMode = releasedKeyCollisionMode;
        body.WakeUp();
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
}
