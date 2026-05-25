using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    [Header("Style")]
    public Sprite backgroundSprite;
    public Sprite titleBarSprite;
    public Sprite buttonSprite;
    public Sprite buttonHighlightedSprite;

    const string LastSceneKey = "GameOverLastScene";
    const string TypeKey = "GameOverType";
    const string DeathType = "Death";
    const string FinishType = "Finish";

    bool isFinish;

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowHUD(false);

        isFinish = PlayerPrefs.GetString(TypeKey, DeathType) == FinishType;
        BuildUI();
    }

    public static void ShowDeath(string sceneName)
    {
        PlayerPrefs.SetString(TypeKey, DeathType);
        PlayerPrefs.SetString(LastSceneKey, sceneName);
        PlayerPrefs.Save();
    }

    public static void ShowFinish()
    {
        PlayerPrefs.SetString(TypeKey, FinishType);
        PlayerPrefs.Save();
    }

    public void ResumeFromCheckpoint()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (PlayerPrefs.HasKey("SavedScene"))
        {
            PlayerHealth.ShouldRestorePosition = true;
            SceneManager.LoadScene(PlayerPrefs.GetString("SavedScene"));
            return;
        }

        string sceneName = PlayerPrefs.GetString(LastSceneKey, "Level01_Entrance");
        SceneManager.LoadScene(sceneName);
    }

    public void NewGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (SaveSystem.Instance != null)
            SaveSystem.Instance.DeleteSave();

        SceneManager.LoadScene("Level01_Entrance");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }

    void BuildUI()
    {
        Canvas canvas = CreateCanvas();
        RectTransform root = canvas.GetComponent<RectTransform>();

        Image dim = CreateImage("Background", root, null);
        dim.color = new Color(0.08f, 0.075f, 0.075f, 0.94f);
        Stretch(dim.rectTransform);

        Image fullBackground = CreateImage("StyledBackground", root, backgroundSprite);
        fullBackground.color = new Color(1f, 1f, 1f, 0.95f);
        fullBackground.preserveAspect = false;
        Overscan(fullBackground.rectTransform, 8f);

        RectTransform content = CreateContainer("Content", root);
        SetRect(content, new Vector2(0f, -5f), new Vector2(640f, 360f));

        Image titleBar = CreateImage("TitleBar", content, titleBarSprite);
        titleBar.type = Image.Type.Sliced;
        SetRect(titleBar.rectTransform, new Vector2(0f, 105f), new Vector2(520f, 82f));

        TMP_Text title = CreateText("Title", titleBar.rectTransform, isFinish ? "Temple Cleared" : "Game Over", 58f);
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color = Color.white;
        Stretch(title.rectTransform);

        TMP_Text subtitle = CreateText("Subtitle", content, isFinish ? "You escaped the abyss" : "Return to your last checkpoint", 24f);
        subtitle.alignment = TextAlignmentOptions.Center;
        subtitle.color = new Color(0.86f, 0.78f, 0.65f, 1f);
        SetRect(subtitle.rectTransform, new Vector2(0f, 45f), new Vector2(520f, 45f));

        UnityAction primaryAction = isFinish ? NewGame : ResumeFromCheckpoint;
        CreateButton("PrimaryButton", content, isFinish ? "New Game" : "Resume", new Vector2(0f, -20f), primaryAction);
        CreateButton("MainMenuButton", content, "Main Menu", new Vector2(0f, -78f), MainMenu);
        CreateButton("QuitButton", content, "Quit", new Vector2(0f, -136f), QuitGame);

        EnsureEventSystem();
    }

    Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("GameOverCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800f, 450f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    void EnsureEventSystem()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();

        inputModule.AssignDefaultActions();
    }

    Button CreateButton(string name, Transform parent, string label, Vector2 position, UnityAction action)
    {
        Image image = CreateImage(name, parent, buttonSprite);
        image.type = Image.Type.Sliced;
        SetRect(image.rectTransform, position, new Vector2(210f, 42f));

        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        TempleAudio.RegisterButton(button);
        button.onClick.AddListener(action);

        if (buttonHighlightedSprite != null)
        {
            SpriteState state = button.spriteState;
            state.highlightedSprite = buttonHighlightedSprite;
            state.pressedSprite = buttonHighlightedSprite;
            state.selectedSprite = buttonHighlightedSprite;
            button.spriteState = state;
        }

        TMP_Text text = CreateText("Text", image.rectTransform, label, 26f);
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        Stretch(text.rectTransform);

        return button;
    }

    Image CreateImage(string name, Transform parent, Sprite sprite)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Image image = obj.AddComponent<Image>();
        image.sprite = sprite;
        return image;
    }

    RectTransform CreateContainer(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        return obj.AddComponent<RectTransform>();
    }

    TMP_Text CreateText(string name, Transform parent, string value, float size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        TMP_Text text = obj.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        return text;
    }

    void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    void Overscan(RectTransform rect, float amount)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(-amount, -amount);
        rect.offsetMax = new Vector2(amount, amount);
    }
}
