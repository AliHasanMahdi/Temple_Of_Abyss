using UnityEngine;

public class AN_HeroInteractive : MonoBehaviour
{
    [Tooltip("Are you have any key?")]
    public bool RedKey = false, BlueKey = false;

    public int RedKeyCount = 0;
    public int BlueKeyCount = 0;

    [Tooltip("Child empty object for plug following")]
    public Transform GoalPosition;

    public int TotalKeyCount
    {
        get
        {
            SyncCountsFromLegacyFlags();
            return RedKeyCount + BlueKeyCount;
        }
    }

    public void AddKey(bool isRedKey)
    {
        SyncCountsFromLegacyFlags();

        if (isRedKey)
        {
            RedKeyCount++;
        }
        else
        {
            BlueKeyCount++;
        }

        SyncLegacyFlagsFromCounts();
    }

    public bool UseKey(bool isRedKey)
    {
        SyncCountsFromLegacyFlags();

        if (isRedKey)
        {
            if (RedKeyCount <= 0) return false;
            RedKeyCount--;
        }
        else
        {
            if (BlueKeyCount <= 0) return false;
            BlueKeyCount--;
        }

        SyncLegacyFlagsFromCounts();
        return true;
    }

    public bool UseAnyKey()
    {
        SyncCountsFromLegacyFlags();

        if (RedKeyCount > 0)
        {
            RedKeyCount--;
            SyncLegacyFlagsFromCounts();
            return true;
        }

        if (BlueKeyCount > 0)
        {
            BlueKeyCount--;
            SyncLegacyFlagsFromCounts();
            return true;
        }

        return false;
    }

    public void SetKeyCounts(int redKeyCount, int blueKeyCount)
    {
        RedKeyCount = Mathf.Max(0, redKeyCount);
        BlueKeyCount = Mathf.Max(0, blueKeyCount);
        SyncLegacyFlagsFromCounts();
    }

    private void SyncCountsFromLegacyFlags()
    {
        if (RedKey && RedKeyCount <= 0)
        {
            RedKeyCount = 1;
        }

        if (BlueKey && BlueKeyCount <= 0)
        {
            BlueKeyCount = 1;
        }
    }

    private void SyncLegacyFlagsFromCounts()
    {
        RedKey = RedKeyCount > 0;
        BlueKey = BlueKeyCount > 0;
    }
}
