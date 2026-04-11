using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    public Slider healthBar;
    public TMP_Text scoreText;
    public TMP_Text checkpointText;

    private TMP_Text controlsText;
    private TMP_Text crosshairText;
    private TMP_Text interactionPromptText;
    private Canvas hudCanvas;
    private int score;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureReferences();
    }

    void Start()
    {
        RefreshScoreText();

        if (checkpointText != null)
            checkpointText.gameObject.SetActive(false);
    }

    void EnsureReferences()
    {
        hudCanvas = FindCanvas("HUDCanvas");
        if (hudCanvas == null)
            hudCanvas = CreateCanvas("HUDCanvas");

        RectTransform canvasRect = hudCanvas.GetComponent<RectTransform>();
        if (canvasRect != null)
            canvasRect.localScale = Vector3.one;

        healthBar ??= FindObjectByName<Slider>("HealthBar");
        scoreText ??= FindObjectByName<TMP_Text>("ScoreText");
        checkpointText ??= FindObjectByName<TMP_Text>("CheckpointText");
        controlsText ??= FindObjectByName<TMP_Text>("ControlsText");
        crosshairText ??= FindObjectByName<TMP_Text>("Crosshair");
        interactionPromptText ??= FindObjectByName<TMP_Text>("InteractionPromptText");

        if (healthBar == null)
            healthBar = CreateHealthBar(hudCanvas.transform);

        if (scoreText == null)
        {
            scoreText = CreateLabel(
                "ScoreText",
                hudCanvas.transform,
                new Vector2(16f, -72f),
                new Vector2(220f, 28f),
                "Score: 0",
                24f,
                TextAlignmentOptions.Left);
        }

        if (checkpointText == null)
        {
            checkpointText = CreateLabel(
                "CheckpointText",
                hudCanvas.transform,
                new Vector2(0f, -40f),
                new Vector2(420f, 36f),
                "Checkpoint Reached!",
                28f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f));
        }

        if (controlsText == null)
        {
            controlsText = CreateLabel(
                "ControlsText",
                hudCanvas.transform,
                new Vector2(16f, 16f),
                new Vector2(360f, 130f),
                "WASD Move\nMouse Look\nShift Sprint\nSpace Jump\nC Crouch\nE Interact\nEsc Pause",
                20f,
                TextAlignmentOptions.BottomLeft,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f));
        }

        if (crosshairText == null)
        {
            crosshairText = CreateLabel(
                "Crosshair",
                hudCanvas.transform,
                Vector2.zero,
                new Vector2(32f, 32f),
                "+",
                28f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f));
        }

        if (interactionPromptText == null)
        {
            interactionPromptText = CreateLabel(
                "InteractionPromptText",
                hudCanvas.transform,
                new Vector2(0f, 120f),
                new Vector2(520f, 40f),
                string.Empty,
                24f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f));
        }

        healthBar.minValue = 0f;
        healthBar.maxValue = 100f;
        healthBar.value = 100f;
        interactionPromptText.gameObject.SetActive(false);
    }

    Canvas FindCanvas(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        return target != null ? target.GetComponent<Canvas>() : null;
    }

    T FindObjectByName<T>(string objectName) where T : Component
    {
        GameObject target = GameObject.Find(objectName);
        return target != null ? target.GetComponent<T>() : null;
    }

    Canvas CreateCanvas(string canvasName)
    {
        GameObject canvasObject = new GameObject(canvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform rect = canvasObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        return canvas;
    }

    Slider CreateHealthBar(Transform parent)
    {
        GameObject sliderObject = new GameObject("HealthBar", typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(parent, false);

        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0f, 1f);
        sliderRect.anchorMax = new Vector2(0f, 1f);
        sliderRect.pivot = new Vector2(0f, 1f);
        sliderRect.anchoredPosition = new Vector2(16f, -16f);
        sliderRect.sizeDelta = new Vector2(280f, 24f);

        GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
        backgroundObject.transform.SetParent(sliderObject.transform, false);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        Image backgroundImage = backgroundObject.GetComponent<Image>();
        backgroundImage.color = new Color(0.1f, 0.1f, 0.14f, 0.9f);

        GameObject fillAreaObject = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaObject.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillAreaObject.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(4f, 4f);
        fillAreaRect.offsetMax = new Vector2(-4f, -4f);

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObject.transform.SetParent(fillAreaObject.transform, false);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = fillObject.GetComponent<Image>();
        fillImage.color = new Color(0.82f, 0.18f, 0.16f, 1f);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    TMP_Text CreateLabel(
        string objectName,
        Transform parent,
        Vector2 anchoredPosition,
        Vector2 size,
        string value,
        float fontSize,
        TextAlignmentOptions alignment,
        Vector2? anchor = null,
        Vector2? pivot = null)
    {
        GameObject labelObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        Vector2 resolvedAnchor = anchor ?? new Vector2(0f, 1f);
        Vector2 resolvedPivot = pivot ?? resolvedAnchor;
        rect.anchorMin = resolvedAnchor;
        rect.anchorMax = resolvedAnchor;
        rect.pivot = resolvedPivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;

        return text;
    }

    public void UpdateHealth(float current, float max)
    {
        EnsureReferences();

        if (healthBar == null || max <= 0f)
            return;

        healthBar.value = Mathf.Clamp((current / max) * 100f, 0f, 100f);
    }

    public void AddScore(int amount)
    {
        score += amount;
        RefreshScoreText();
    }

    void RefreshScoreText()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public int GetScore()
    {
        return score;
    }

    public void ShowCheckpointMessage()
    {
        if (checkpointText == null)
            return;

        StopAllCoroutines();
        StartCoroutine(ShowMessage());
    }

    IEnumerator ShowMessage()
    {
        checkpointText.text = "Checkpoint Reached!";
        checkpointText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        checkpointText.gameObject.SetActive(false);
    }

    public void ShowInteractionPrompt(string prompt)
    {
        EnsureReferences();

        if (interactionPromptText == null)
            return;

        interactionPromptText.text = prompt;
        interactionPromptText.gameObject.SetActive(!string.IsNullOrEmpty(prompt));
    }

    public void HideInteractionPrompt()
    {
        if (interactionPromptText != null)
            interactionPromptText.gameObject.SetActive(false);
    }
}
