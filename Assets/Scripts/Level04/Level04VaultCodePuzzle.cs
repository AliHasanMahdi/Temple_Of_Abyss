using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Level04VaultCodePuzzle : MonoBehaviour
{
    [SerializeField] TMP_Text[] slotTexts = new TMP_Text[5];
    [SerializeField] GameObject treasureChestObject;
    [SerializeField] Level04VaultPuzzleInteractable submitInteractable;
    [SerializeField] AudioClip solvedSound;
    [SerializeField] Vector3 solvedChestPosition = new Vector3(-6.656023f, 9.354f, -18.86613f);
    [SerializeField] Vector3 solvedChestEulerAngles = new Vector3(5.635f, -2.782f, -86.195f);

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
            HUDManager.Instance.ShowTimedMessage(bulls + " bulls, " + cows + " cows", 2f);
    }

    void CompletePuzzle()
    {
        isSolved = true;
        PlaySolvedSound();
        TeleportTreasureChest();
        DisableSubmitInteraction();

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowTimedMessage("Vault code accepted!", 2f);
    }

    void InitializeState()
    {
        currentDigits = new int[slotTexts.Length];
        defaultSlotColors = new Color[slotTexts.Length];
        rememberedStates = CreateStateCache(slotTexts.Length);
        secretNumber ??= BullsAndCowsGame.GenerateSecretNumber(slotTexts.Length);

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

        TempleAudio.PlaySfx(solvedSound, 0.9f);
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

    static Dictionary<int, BullsAndCowsSlotState>[] CreateStateCache(int count)
    {
        Dictionary<int, BullsAndCowsSlotState>[] stateCache = new Dictionary<int, BullsAndCowsSlotState>[count];
        for (int i = 0; i < count; i++)
            stateCache[i] = new Dictionary<int, BullsAndCowsSlotState>();

        return stateCache;
    }

    static Color GetColorForState(BullsAndCowsSlotState state)
    {
        return state switch
        {
            BullsAndCowsSlotState.CorrectNumberCorrectPosition => new Color32(72, 185, 84, 255),
            BullsAndCowsSlotState.CorrectNumberWrongPosition => new Color32(232, 145, 46, 255),
            _ => new Color32(201, 62, 62, 255)
        };
    }
}
