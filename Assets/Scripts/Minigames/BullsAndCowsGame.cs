using System.Collections.Generic;
using UnityEngine;

public enum BullsAndCowsSlotState
{
    WrongNumber,
    CorrectNumberWrongPosition,
    CorrectNumberCorrectPosition
}

public static class BullsAndCowsGame
{
    const int DefaultSlotCount = 5;
    const int MaxUniqueDigits = 10;

    public static Dictionary<int, int> GenerateSecretNumber(int slotCount = DefaultSlotCount)
    {
        int clampedSlotCount = Mathf.Clamp(slotCount, 1, MaxUniqueDigits);
        List<int> digits = new List<int>(clampedSlotCount);

        while (digits.Count < clampedSlotCount)
        {
            int nextDigit = Random.Range(0, MaxUniqueDigits);
            if (!digits.Contains(nextDigit))
                digits.Add(nextDigit);
        }

        Dictionary<int, int> result = new Dictionary<int, int>(clampedSlotCount);
        for (int i = 0; i < digits.Count; i++)
            result[i] = digits[i];

        return result;
    }

    public static Dictionary<int, BullsAndCowsSlotState> EvaluateGuess(Dictionary<int, int> secret, Dictionary<int, int> guess)
    {
        Dictionary<int, BullsAndCowsSlotState> result = new Dictionary<int, BullsAndCowsSlotState>();
        if (secret == null || guess == null)
            return result;

        HashSet<int> secretDigits = new HashSet<int>(secret.Values);
        foreach (KeyValuePair<int, int> pair in guess)
        {
            if (secret.TryGetValue(pair.Key, out int secretValue) && secretValue == pair.Value)
            {
                result[pair.Key] = BullsAndCowsSlotState.CorrectNumberCorrectPosition;
            }
            else if (secretDigits.Contains(pair.Value))
            {
                result[pair.Key] = BullsAndCowsSlotState.CorrectNumberWrongPosition;
            }
            else
            {
                result[pair.Key] = BullsAndCowsSlotState.WrongNumber;
            }
        }

        return result;
    }

    public static int Increase(int value)
    {
        return value + 1;
    }

    public static int Decrease(int value)
    {
        return value - 1;
    }
}
