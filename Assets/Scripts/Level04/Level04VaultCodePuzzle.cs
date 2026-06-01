using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Level04VaultCodePuzzle : MonoBehaviour
{
    const float ReleasedKeyDropKick = 0.65f;
    const float ReleasedKeyDropOffset = 0.08f;
    const float ReleasedKeyOutwardOffset = 0.12f;

    [Header("References")]
    [SerializeField] TMP_Text[] slotTexts = new TMP_Text[5];
    [SerializeField] GameObject treasureChestObject;
    [SerializeField] GameObject releasedDoorKeyObject;
    [SerializeField] Level04VaultPuzzleInteractable submitInteractable;

    [Header("Solved Result")]
    [SerializeField] AudioClip solvedSound;
    [SerializeField] float solvedSoundVolume = 0.9f;
    [SerializeField] Vector3 solvedChestPosition = new Vector3(-6.656023f, 9.354f, -18.86613f);
    [SerializeField] Vector3 solvedChestEulerAngles = new Vector3(5.635f, -2.782f, -86.195f);
    [SerializeField] bool disableSubmitAfterSolve = true;

    [Header("Messages")]
    [SerializeField] string progressMessageFormat = "{0} bulls, {1} cows";
    [SerializeField] float progressMessageDuration = 2f;
    [SerializeField] string solvedMessage = "Vault code accepted!";
    [SerializeField] float solvedMessageDuration = 2f;

    [Header("Slot Colors")]
    [SerializeField] Color correctPositionColor = new Color32(72, 185, 84, 255);
    [SerializeField] Color correctNumberWrongPositionColor = new Color32(232, 145, 46, 255);
    [SerializeField] Color wrongNumberColor = new Color32(201, 62, 62, 255);

    [Header("Secret")]
    [SerializeField] bool useCustomSecret;
    [SerializeField] int[] customSecretDigits = new int[5];

    [Header("Released Key Physics")]
    [SerializeField] bool detachReleasedKey = true;
    [SerializeField] bool releasedKeyTrigger = false;
    [SerializeField] bool releasedKeyUseGravity = true;
    [SerializeField] bool releasedKeyIsKinematic = false;
    [SerializeField] bool resetReleasedKeyVelocity = true;
    [SerializeField] CollisionDetectionMode releasedKeyCollisionMode = CollisionDetectionMode.ContinuousDynamic;

    int[] currentDigits;
    Color[] defaultSlotColors;
    Dictionary<int, BullsAndCowsSlotState>[] rememberedStates;
    Dictionary<int, int> secretNumber;
    bool isSolved;

    void Awake()
    {
        InitializeState();
        ApplyAllDigits();
    }

    public void IncreaseSlot(int slotIndex)
    {
        if (isSolved || !IsValidSlot(slotIndex))
            return;

        currentDigits[slotIndex] = BullsAndCowsGame.Increase(currentDigits[slotIndex]);
        ApplyDigit(slotIndex);
    }

    public void DecreaseSlot(int slotIndex)
    {
        if (isSolved || !IsValidSlot(slotIndex))
            return;

        currentDigits[slotIndex] = BullsAndCowsGame.Decrease(currentDigits[slotIndex]);
        ApplyDigit(slotIndex);
    }

    public void SubmitGuess()
    {
        if (isSolved || slotTexts == null || slotTexts.Length == 0)
            return;

        Dictionary<int, int> guess = new Dictionary<int, int>(slotTexts.Length);
        for (int i = 0; i < slotTexts.Length; i++)
            guess[i] = currentDigits[i];

        Dictionary<int, BullsAndCowsSlotState> result = BullsAndCowsGame.EvaluateGuess(secretNumber, guess);

        int bulls = 0;
        int cows = 0;
        for (int i = 0; i < slotTexts.Length; i++)
        {
            BullsAndCowsSlotState state = result.TryGetValue(i, out BullsAndCowsSlotState foundState)
                ? foundState
                : BullsAndCowsSlotState.WrongNumber;

            rememberedStates[i][currentDigits[i]] = state;
            ApplyStateColor(i, state);

            if (state == BullsAndCowsSlotState.CorrectNumberCorrectPosition)
                bulls++;
            else if (state == BullsAndCowsSlotState.CorrectNumberWrongPosition)
                cows++;
        }

        if (bulls == slotTexts.Length)
        {
            CompletePuzzle();
            return;
        }

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowTimedMessage(
                string.Format(progressMessageFormat, bulls, cows),
                progressMessageDuration);
    }

    void CompletePuzzle()
    {
        isSolved = true;
        PlaySolvedSound();
        TeleportTreasureChest();
        ReleaseDoorKey();
        if (disableSubmitAfterSolve)
            DisableSubmitInteraction();

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowTimedMessage(solvedMessage, solvedMessageDuration);
    }

    void InitializeState()
    {
        currentDigits = new int[slotTexts.Length];
        defaultSlotColors = new Color[slotTexts.Length];
        rememberedStates = CreateStateCache(slotTexts.Length);
        secretNumber ??= CreateSecretNumber(slotTexts.Length);

        for (int i = 0; i < slotTexts.Length; i++)
        {
            TMP_Text text = slotTexts[i];
            if (text == null)
                continue;

            defaultSlotColors[i] = text.color;
            currentDigits[i] = ParseDigit(text.text);
        }
    }

    void ApplyAllDigits()
    {
        for (int i = 0; i < slotTexts.Length; i++)
            ApplyDigit(i);
    }

    void ApplyDigit(int slotIndex)
    {
        if (!IsValidSlot(slotIndex) || slotTexts[slotIndex] == null)
            return;

        slotTexts[slotIndex].text = currentDigits[slotIndex].ToString();
        ApplyRememberedColor(slotIndex);
    }

    void ApplyRememberedColor(int slotIndex)
    {
        if (!IsValidSlot(slotIndex) || slotTexts[slotIndex] == null)
            return;

        if (rememberedStates[slotIndex].TryGetValue(currentDigits[slotIndex], out BullsAndCowsSlotState state))
            ApplyStateColor(slotIndex, state);
        else
            slotTexts[slotIndex].color = defaultSlotColors[slotIndex];
    }

    void ApplyStateColor(int slotIndex, BullsAndCowsSlotState state)
    {
        if (!IsValidSlot(slotIndex) || slotTexts[slotIndex] == null)
            return;

        slotTexts[slotIndex].color = GetColorForState(state);
    }

    void TeleportTreasureChest()
    {
        if (treasureChestObject == null)
            return;

        Transform chestTransform = treasureChestObject.transform;
        Rigidbody chestBody = treasureChestObject.GetComponent<Rigidbody>();
        if (chestBody != null)
        {
            chestBody.linearVelocity = Vector3.zero;
            chestBody.angularVelocity = Vector3.zero;
            chestBody.position = solvedChestPosition;
            chestBody.rotation = Quaternion.Euler(solvedChestEulerAngles);
            chestBody.WakeUp();
        }
        else
        {
            chestTransform.SetPositionAndRotation(solvedChestPosition, Quaternion.Euler(solvedChestEulerAngles));
        }
    }

    void DisableSubmitInteraction()
    {
        if (submitInteractable == null)
            return;

        submitInteractable.enabled = false;

        Collider[] colliders = submitInteractable.GetComponents<Collider>();
        foreach (Collider collider in colliders)
            collider.enabled = false;
    }

    void PlaySolvedSound()
    {
        if (solvedSound == null)
            return;

        TempleAudio.PlaySfx(solvedSound, solvedSoundVolume);
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

    bool IsValidSlot(int slotIndex)
    {
        return slotTexts != null && slotIndex >= 0 && slotIndex < slotTexts.Length;
    }

    static int ParseDigit(string value)
    {
        if (!int.TryParse(value, out int digit))
            return 0;

        return Mathf.Clamp(digit, 0, 9);
    }

    Dictionary<int, int> CreateSecretNumber(int slotCount)
    {
        if (!useCustomSecret)
            return BullsAndCowsGame.GenerateSecretNumber(slotCount);

        Dictionary<int, int> secret = new Dictionary<int, int>(slotCount);
        HashSet<int> usedDigits = new HashSet<int>();

        for (int i = 0; i < slotCount; i++)
        {
            int digit = i < customSecretDigits.Length ? customSecretDigits[i] : 0;
            digit = Mathf.Clamp(digit, 0, 9);

            if (usedDigits.Contains(digit))
                return BullsAndCowsGame.GenerateSecretNumber(slotCount);

            usedDigits.Add(digit);
            secret[i] = digit;
        }

        return secret;
    }

    static Dictionary<int, BullsAndCowsSlotState>[] CreateStateCache(int count)
    {
        Dictionary<int, BullsAndCowsSlotState>[] stateCache = new Dictionary<int, BullsAndCowsSlotState>[count];
        for (int i = 0; i < count; i++)
            stateCache[i] = new Dictionary<int, BullsAndCowsSlotState>();

        return stateCache;
    }

    Color GetColorForState(BullsAndCowsSlotState state)
    {
        return state switch
        {
            BullsAndCowsSlotState.CorrectNumberCorrectPosition => correctPositionColor,
            BullsAndCowsSlotState.CorrectNumberWrongPosition => correctNumberWrongPositionColor,
            _ => wrongNumberColor
        };
    }
}
