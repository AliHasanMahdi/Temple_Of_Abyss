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

            if (slotCounts != null && i < slotCounts.Length && slotCounts[i] != null)
            {
                slotCounts[i].enabled = hasKey;
                slotCounts[i].text = hasKey ? $"{keyCount}x" : string.Empty;
            }
        }
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
