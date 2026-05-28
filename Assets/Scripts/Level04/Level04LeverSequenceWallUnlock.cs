using System.Collections.Generic;
using UnityEngine;

public class Level04LeverSequenceWallUnlock : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] int requiredLeverCount = 5;
    [SerializeField] string[] fallbackLeverNames =
    {
        "Lever_Prefab (1)",
        "Lever_Prefab (2)",
        "Lever_Prefab (3)",
        "Lever_Prefab (4)",
        "Lever_Prefab"
    };

    [Header("Wall")]
    [SerializeField] GameObject targetWall;
    [SerializeField] GameObject releasedDoorKeyObject;
    [SerializeField] bool enableGravity = true;
    [SerializeField] bool startKinematic = false;

    [Header("Audio")]
    [SerializeField] AudioClip unlockSound;
    [SerializeField] float unlockVolume = 0.9f;

    [Header("Debug")]
    [SerializeField] bool[] generatedSequence;

    AN_Button[] levers;
    bool checkingEnabled = true;

    void Awake()
    {
        if (targetWall == null)
            targetWall = gameObject;

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

        UnlockWall();
    }

    void ResolveLevers()
    {
        List<AN_Button> foundLevers = new List<AN_Button>(fallbackLeverNames.Length);
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

        if (foundLevers.Count >= 5)
            requiredLeverCount = 5;
        else
            requiredLeverCount = Mathf.Min(4, foundLevers.Count);

        levers = foundLevers.GetRange(0, requiredLeverCount).ToArray();
    }

    void GenerateSequence()
    {
        if (requiredLeverCount <= 0)
        {
            generatedSequence = System.Array.Empty<bool>();
            checkingEnabled = false;
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

        Debug.Log("[Level04LeverSequenceWallUnlock] Sequence: " + BuildSequenceDebugString());
    }

    void UnlockWall()
    {
        checkingEnabled = false;

        if (targetWall != null)
        {
            Rigidbody body = targetWall.GetComponent<Rigidbody>();
            if (body == null)
                body = targetWall.AddComponent<Rigidbody>();

            body.isKinematic = startKinematic;
            body.useGravity = enableGravity;
        }

        ReleaseDoorKey();

        if (unlockSound != null)
            TempleAudio.PlaySfx(unlockSound, unlockVolume);
    }

    void ReleaseDoorKey()
    {
        if (releasedDoorKeyObject == null)
            return;

        releasedDoorKeyObject.transform.SetParent(null, true);

        Collider collider = releasedDoorKeyObject.GetComponent<Collider>();
        if (collider != null)
            collider.isTrigger = false;

        Rigidbody body = releasedDoorKeyObject.GetComponent<Rigidbody>();
        if (body == null)
            body = releasedDoorKeyObject.AddComponent<Rigidbody>();

        body.isKinematic = false;
        body.useGravity = true;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
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
