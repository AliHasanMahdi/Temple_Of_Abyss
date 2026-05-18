using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public static class InventoryRuntimeInstaller
{
    private const int Columns = 9;
    private const int Rows = 4;
    private const string InventoryResourcesPath = "InventoryUI/";
    private const string GuiPartsPath = "Assets/GUI_Parts/Gui_parts/";

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
        if (ShouldSkipInventory(SceneManager.GetActiveScene().name))
        {
            RemoveInventory(existingManager);
            return;
        }

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
        InventorySprites sprites = LoadInventorySprites();

        GameObject dim = CreateImage("InventoryDim", canvasObject.transform, new Color(0f, 0f, 0f, 0.18f));
        StretchToParent(dim.GetComponent<RectTransform>());

        GameObject panel = CreateImage("InventoryPanel", canvasObject.transform, new Color(0.32f, 0.34f, 0.38f, 0.82f));
        ConfigureImage(panel.GetComponent<Image>(), sprites.InventoryBackground, Color.white);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(940f, 608f);
        panelRect.anchoredPosition = Vector2.zero;

        GameObject titleBar = CreateImage("InventoryTitleBar", panel.transform, new Color(0.16f, 0.13f, 0.1f, 0.9f));
        ConfigureImage(titleBar.GetComponent<Image>(), sprites.TitleBackground, Color.white);
        RectTransform titleBarRect = titleBar.GetComponent<RectTransform>();
        titleBarRect.anchorMin = new Vector2(0f, 1f);
        titleBarRect.anchorMax = new Vector2(1f, 1f);
        titleBarRect.pivot = new Vector2(0.5f, 1f);
        titleBarRect.offsetMin = new Vector2(50f, -116f);
        titleBarRect.offsetMax = new Vector2(-50f, -42f);

        GameObject title = CreateText("InventoryTitle", titleBar.transform, "Inventory", 48, TextAlignmentOptions.Center);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        StretchToParent(titleRect);
        titleRect.offsetMin = new Vector2(24f, 8f);
        titleRect.offsetMax = new Vector2(-24f, -10f);

        GameObject grid = new GameObject("InventoryGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        grid.transform.SetParent(panel.transform, false);
        RectTransform gridRect = grid.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0f, 0f);
        gridRect.anchorMax = new Vector2(1f, 1f);
        gridRect.offsetMin = new Vector2(28f, 48f);
        gridRect.offsetMax = new Vector2(-28f, -132f);

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
            GameObject slot = CreateSlot(grid.transform, sprites);
            slotIcons.Add(CreateSlotIcon(slot.transform));
            slotCounts.Add(CreateSlotCount(slot.transform));
        }

        manager.inventoryUI = canvasObject;
        manager.gridGen = null;
        manager.ConfigureInventorySlots(slotIcons.ToArray(), slotCounts.ToArray(), keySprite);
        canvasObject.SetActive(false);
    }

    private static bool ShouldSkipInventory(string sceneName)
    {
        return sceneName == "MainMenu" || sceneName == "GameOver";
    }

    private static void RemoveInventory(InventoryManager manager)
    {
        if (manager == null)
        {
            return;
        }

        if (manager.inventoryUI != null)
        {
            Object.Destroy(manager.inventoryUI);
            manager.inventoryUI = null;
            manager.gridGen = null;
        }

        if (manager.gameObject.name == "InventorySystem")
        {
            Object.Destroy(manager.gameObject);
        }
        else
        {
            Object.Destroy(manager);
        }
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

    private static GameObject CreateSlot(Transform parent, InventorySprites sprites)
    {
        GameObject slot = CreateImage("Slot", parent, new Color(0.12f, 0.15f, 0.19f, 0.38f));
        ConfigureImage(slot.GetComponent<Image>(), sprites.CellBackground, Color.white);

        GameObject frame = CreateImage("SlotFrame", slot.transform, Color.white);
        StretchToParent(frame.GetComponent<RectTransform>());
        Image frameImage = frame.GetComponent<Image>();
        ConfigureImage(frameImage, sprites.CellFrame, Color.white);
        frameImage.raycastTarget = false;

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

    private static void ConfigureImage(Image image, Sprite sprite, Color spriteColor)
    {
        if (image == null || sprite == null)
        {
            return;
        }

        image.sprite = sprite;
        image.color = spriteColor;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
    }

    private static InventorySprites LoadInventorySprites()
    {
        return new InventorySprites
        {
            TitleBackground = LoadGuiSprite("name_bar2"),
            InventoryBackground = LoadGuiSprite("barmid_ready"),
            CellBackground = LoadGuiSprite("Mini_background"),
            CellFrame = LoadGuiSprite("Mini_frame1")
        };
    }

    private static Sprite LoadGuiSprite(string assetName)
    {
        Sprite sprite = Resources.Load<Sprite>($"{InventoryResourcesPath}{assetName}");
        if (sprite != null)
        {
            return sprite;
        }

        sprite = Resources.Load<Sprite>(assetName);
        if (sprite != null)
        {
            return sprite;
        }

#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<Sprite>($"{GuiPartsPath}{assetName}.png");
#else
        return null;
#endif
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

    private sealed class InventorySprites
    {
        public Sprite TitleBackground;
        public Sprite InventoryBackground;
        public Sprite CellBackground;
        public Sprite CellFrame;
    }
}
