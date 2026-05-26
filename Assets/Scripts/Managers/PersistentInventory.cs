using System.Collections.Generic;
using UnityEngine;

public static class PersistentInventory
{
    public const string EmeraldKey = "level05_key_emerald";
    public const string SapphireKey = "level05_key_sapphire";
    public const string RubyKey = "level05_key_ruby";
    public const string SunKey = "level05_key_sun";

    public const string EmeraldGem = "level05_gem_emerald";
    public const string SapphireGem = "level05_gem_sapphire";
    public const string RubyGem = "level05_gem_ruby";
    public const string SunGem = "level05_gem_sun";

    public const string EmeraldPlaced = "level05_placed_emerald";
    public const string SapphirePlaced = "level05_placed_sapphire";
    public const string RubyPlaced = "level05_placed_ruby";
    public const string SunPlaced = "level05_placed_sun";

    public const string GoldenStatue = "level05_golden_statue";

    private static readonly string[] KnownIds =
    {
        EmeraldKey,
        SapphireKey,
        RubyKey,
        SunKey,
        EmeraldGem,
        SapphireGem,
        RubyGem,
        SunGem,
        EmeraldPlaced,
        SapphirePlaced,
        RubyPlaced,
        SunPlaced,
        GoldenStatue
    };

    public struct DisplayItem
    {
        public string Id;
        public string Label;
        public Color Color;

        public DisplayItem(string id, string label, Color color)
        {
            Id = id;
            Label = label;
            Color = color;
        }
    }

    public static bool Has(string id)
    {
        return PlayerPrefs.GetInt(Key(id), 0) == 1;
    }

    public static void Collect(string id)
    {
        PlayerPrefs.SetInt(Key(id), 1);
        PlayerPrefs.SetInt(CollectedKey(id), 1);
        PlayerPrefs.Save();
    }

    public static bool WasCollected(string id)
    {
        return PlayerPrefs.GetInt(CollectedKey(id), 0) == 1;
    }

    public static bool Consume(string id)
    {
        if (!Has(id))
        {
            return false;
        }

        PlayerPrefs.SetInt(Key(id), 0);
        PlayerPrefs.Save();
        return true;
    }

    public static void MarkPlaced(string placedId)
    {
        PlayerPrefs.SetInt(Key(placedId), 1);
        PlayerPrefs.Save();
    }

    public static bool AllOfferingsPlaced()
    {
        return Has(EmeraldPlaced) && Has(SapphirePlaced) && Has(RubyPlaced) && Has(SunPlaced);
    }

    public static bool IsGemResolved(string gemId)
    {
        switch (gemId)
        {
            case EmeraldGem:
                return Has(EmeraldGem) || Has(EmeraldPlaced);
            case SapphireGem:
                return Has(SapphireGem) || Has(SapphirePlaced);
            case RubyGem:
                return Has(RubyGem) || Has(RubyPlaced);
            case SunGem:
                return Has(SunGem) || Has(SunPlaced);
            default:
                return Has(gemId);
        }
    }

    public static List<DisplayItem> GetDisplayItems()
    {
        List<DisplayItem> items = new List<DisplayItem>();
        AddIfHeld(items, EmeraldKey, "EK", new Color(0.2f, 0.9f, 0.35f));
        AddIfHeld(items, SapphireKey, "SK", new Color(0.25f, 0.45f, 1f));
        AddIfHeld(items, RubyKey, "RK", new Color(1f, 0.2f, 0.18f));
        AddIfHeld(items, SunKey, "YK", new Color(1f, 0.85f, 0.15f));
        AddIfHeld(items, EmeraldGem, "EG", new Color(0.2f, 0.9f, 0.35f));
        AddIfHeld(items, SapphireGem, "SG", new Color(0.25f, 0.45f, 1f));
        AddIfHeld(items, RubyGem, "RG", new Color(1f, 0.2f, 0.18f));
        AddIfHeld(items, SunGem, "YG", new Color(1f, 0.85f, 0.15f));
        AddIfHeld(items, GoldenStatue, "GS", new Color(1f, 0.72f, 0.18f));
        return items;
    }

    public static void ClearAll()
    {
        foreach (string id in KnownIds)
        {
            PlayerPrefs.DeleteKey(Key(id));
            PlayerPrefs.DeleteKey(CollectedKey(id));
        }

        PlayerPrefs.Save();
    }

    private static void AddIfHeld(List<DisplayItem> items, string id, string label, Color color)
    {
        if (Has(id))
        {
            items.Add(new DisplayItem(id, label, color));
        }
    }

    private static string Key(string id)
    {
        return "Inventory_" + id;
    }

    private static string CollectedKey(string id)
    {
        return "InventoryCollected_" + id;
    }
}
