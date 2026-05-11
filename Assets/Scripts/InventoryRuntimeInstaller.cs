using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public static class InventoryRuntimeInstaller
{
    private const int Columns = 9;
    private const int Rows = 4;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        InstallInventory();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InstallInventory();
    }

    private static void InstallInventory()
    {
        InventoryManager existingManager = Object.FindObjectOfType<InventoryManager>();
        if (existingManager != null &&
            existingManager.inventoryUI != null &&
            existingManager.inventoryUI.GetComponentInParent<Canvas>() != null)
        {
            return;
        }

        if (existingManager != null && existingManager.inventoryUI != null)
        {
            Object.Destroy(existingManager.inventoryUI);
            existingManager.inventoryUI = null;
            existingManager.gridGen = null;
        }

        EnsureEventSystem();

        InventoryManager manager;
        if (existingManager != null)
        {
            manager = existingManager;
        }
        else
        {
            GameObject root = new GameObject("InventorySystem");
            manager = root.AddComponent<InventoryManager>();
        }

        manager.pauseGameWhenOpen = true;

        GameObject canvasObject = new GameObject("InventoryUI");
        canvasObject.transform.SetParent(manager.transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject dim = CreateImage("InventoryDim", canvasObject.transform, new Color(0f, 0f, 0f, 0.18f));
        StretchToParent(dim.GetComponent<RectTransform>());

        GameObject panel = CreateImage("InventoryPanel", canvasObject.transform, new Color(0.32f, 0.34f, 0.38f, 0.82f));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(940f, 608f);
        panelRect.anchoredPosition = Vector2.zero;

        GameObject title = CreateText("InventoryTitle", panel.transform, "Inventory", 54, TextAlignmentOptions.Left);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.offsetMin = new Vector2(24f, -96f);
        titleRect.offsetMax = new Vector2(-24f, -22f);

        GameObject divider = CreateImage("InventoryDivider", panel.transform, new Color(1f, 1f, 1f, 0.9f));
        RectTransform dividerRect = divider.GetComponent<RectTransform>();
        dividerRect.anchorMin = new Vector2(0f, 1f);
        dividerRect.anchorMax = new Vector2(1f, 1f);
        dividerRect.pivot = new Vector2(0.5f, 1f);
        dividerRect.offsetMin = new Vector2(20f, -94f);
        dividerRect.offsetMax = new Vector2(-20f, -91f);

        GameObject grid = new GameObject("InventoryGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        grid.transform.SetParent(panel.transform, false);
        RectTransform gridRect = grid.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0f, 0f);
        gridRect.anchorMax = new Vector2(1f, 1f);
        gridRect.offsetMin = new Vector2(28f, 48f);
        gridRect.offsetMax = new Vector2(-28f, -112f);

        GridLayoutGroup layout = grid.GetComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = Columns;
        layout.cellSize = new Vector2(84f, 84f);
        layout.spacing = new Vector2(16f, 22f);
        layout.childAlignment = TextAnchor.UpperLeft;

        Sprite keySprite = LoadKeySprite();
        List<Image> slotIcons = new List<Image>();
        List<TMP_Text> slotCounts = new List<TMP_Text>();

        for (int i = 0; i < Columns * Rows; i++)
        {
            GameObject slot = CreateSlot(grid.transform);
            slotIcons.Add(CreateSlotIcon(slot.transform));
            slotCounts.Add(CreateSlotCount(slot.transform));
        }

        manager.inventoryUI = canvasObject;
        manager.gridGen = null;
        manager.ConfigureInventorySlots(slotIcons.ToArray(), slotCounts.ToArray(), keySprite);
        canvasObject.SetActive(false);
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        eventSystem.AddComponent<InputSystemUIInputModule>();
#else
        eventSystem.AddComponent<StandaloneInputModule>();
#endif
    }

    private static GameObject CreateSlot(Transform parent)
    {
        GameObject slot = CreateImage("Slot", parent, new Color(0.12f, 0.15f, 0.19f, 0.38f));
        Outline outline = slot.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.92f);
        outline.effectDistance = new Vector2(2f, -2f);
        return slot;
    }

    private static Image CreateSlotIcon(Transform slot)
    {
        GameObject iconObject = CreateImage("KeyIcon", slot, Color.white);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        StretchToParent(iconRect);
        iconRect.offsetMin = new Vector2(8f, 8f);
        iconRect.offsetMax = new Vector2(-8f, -8f);

        Image icon = iconObject.GetComponent<Image>();
        icon.preserveAspect = true;
        icon.enabled = false;
        return icon;
    }

    private static TMP_Text CreateSlotCount(Transform slot)
    {
        GameObject countObject = CreateText("ItemCount", slot, string.Empty, 28, TextAlignmentOptions.BottomRight);
        RectTransform countRect = countObject.GetComponent<RectTransform>();
        StretchToParent(countRect);
        countRect.offsetMin = new Vector2(4f, 2f);
        countRect.offsetMax = new Vector2(-6f, -4f);

        TMP_Text count = countObject.GetComponent<TMP_Text>();
        count.enabled = false;
        return count;
    }

    private static GameObject CreateImage(string name, Transform parent, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.color = color;
        return obj;
    }

    private static Sprite LoadKeySprite()
    {
        Sprite sprite = Resources.Load<Sprite>("key photo");
        if (sprite != null)
        {
            return sprite;
        }

        Texture2D texture = Resources.Load<Texture2D>("key photo");
        if (texture == null)
        {
            Debug.LogWarning("InventoryRuntimeInstaller: key photo.png was not found in Assets/Resources.");
            return null;
        }

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    private static GameObject CreateText(string name, Transform parent, string value, int size, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);

        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        text.outlineWidth = 0.18f;
        text.outlineColor = Color.black;

        return obj;
    }

    private static void StretchToParent(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

}
