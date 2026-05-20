using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public GameObject inventoryUI;
    public GridGenerator gridGen;
    public KeyCode legacyToggleKey = KeyCode.I;
    public bool pauseGameWhenOpen = true;

    private bool isOpen;
    private CursorLockMode previousLockState;
    private bool previousCursorVisible;
    private float previousTimeScale = 1f;
    private Image[] slotIcons;
    private TMP_Text[] slotCounts;
    private Sprite keySprite;
    private Sprite questItemSprite;
    private int keyCount;

    void Awake()
    {
        Instance = this;
        EnsureInventoryReferences();
    }

    void Start()
    {
        SetInventoryOpen(false);
    }

    void Update()
    {
        if (WasTogglePressed())
        {
            SetInventoryOpen(!isOpen);
        }
    }

    public void SetInventoryOpen(bool open)
    {
        EnsureInventoryReferences();
        SyncKeysFromHero();

        if (inventoryUI == null)
        {
            return;
        }

        if (!open && !isOpen)
        {
            inventoryUI.SetActive(false);
            return;
        }

        if (open && !isOpen)
        {
            previousLockState = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            previousTimeScale = Time.timeScale;
        }

        isOpen = open;
        inventoryUI.SetActive(isOpen);

        if (isOpen)
        {
            if (gridGen != null)
            {
                gridGen.GenerateGrid();
            }

            RefreshSlots();

            if (pauseGameWhenOpen)
            {
                Time.timeScale = 0f;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (pauseGameWhenOpen)
            {
                Time.timeScale = previousTimeScale;
            }

            Cursor.lockState = previousLockState;
            Cursor.visible = previousCursorVisible;
        }
    }

    public void ConfigureInventorySlots(Image[] icons, TMP_Text[] counts, Sprite keyIcon)
    {
        slotIcons = icons;
        slotCounts = counts;
        keySprite = keyIcon;
        RefreshSlots();
    }

    public void AddKey(bool isRedKey)
    {
        SyncKeysFromHero();
    }

    public void RemoveKey(bool isRedKey)
    {
        SyncKeysFromHero();
    }

    public void RemoveAnyKey()
    {
        SyncKeysFromHero();
    }

    public void SyncKeysFromPlayer()
    {
        SyncKeysFromHero();
    }

    public void RefreshInventory()
    {
        SyncKeysFromHero();
        RefreshSlots();
    }

    private void EnsureInventoryReferences()
    {
        if (inventoryUI == null)
        {
            inventoryUI = transform.Find("InventoryUI")?.gameObject;
        }

        if (gridGen == null && inventoryUI != null)
        {
            gridGen = inventoryUI.GetComponentInChildren<GridGenerator>(true);
        }
    }

    private void SyncKeysFromHero()
    {
        AN_HeroInteractive hero = FindObjectOfType<AN_HeroInteractive>();
        if (hero == null)
        {
            return;
        }

        int heroKeyCount = hero.TotalKeyCount;

        if (heroKeyCount != keyCount)
        {
            keyCount = heroKeyCount;
            RefreshSlots();
        }
    }

    private void RefreshSlots()
    {
        if (slotIcons == null)
        {
            return;
        }

        for (int i = 0; i < slotIcons.Length; i++)
        {
            Image icon = slotIcons[i];
            if (icon == null)
            {
                continue;
            }

            bool hasKey = i == 0 && keyCount > 0;
            icon.enabled = hasKey && keySprite != null;
            icon.sprite = hasKey ? keySprite : null;
            icon.color = Color.white;

            if (slotCounts != null && i < slotCounts.Length && slotCounts[i] != null)
            {
                slotCounts[i].enabled = hasKey;
                slotCounts[i].text = hasKey ? $"{keyCount}x" : string.Empty;
            }
        }

        int slotIndex = keyCount > 0 ? 1 : 0;
        foreach (PersistentInventory.DisplayItem item in PersistentInventory.GetDisplayItems())
        {
            if (slotIndex >= slotIcons.Length)
            {
                break;
            }

            Image icon = slotIcons[slotIndex];
            if (icon != null)
            {
                icon.enabled = true;
                icon.sprite = GetQuestItemSprite();
                icon.color = item.Color;
            }

            if (slotCounts != null && slotIndex < slotCounts.Length && slotCounts[slotIndex] != null)
            {
                slotCounts[slotIndex].enabled = true;
                slotCounts[slotIndex].text = item.Label;
            }

            slotIndex++;
        }
    }

    private Sprite GetQuestItemSprite()
    {
        if (questItemSprite != null)
        {
            return questItemSprite;
        }

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        questItemSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return questItemSprite;
    }

    private bool WasTogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null &&
            (Keyboard.current.iKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame))
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(legacyToggleKey) || Input.GetKeyDown(KeyCode.Tab);
#else
        return false;
#endif
    }
}
