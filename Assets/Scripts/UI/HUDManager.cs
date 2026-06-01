using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("Health Bar")]
    public Image hpFill;
    public Slider healthBar;

    [Header("Score")]
    public Image scoreImage;
    public TMP_Text scoreText;

    [Header("Messages")]
    public TMP_Text checkpointText;
    public GameObject messageBackground;

    [Header("Interact Prompt")]
    public TMP_Text interactPromptText;
    public GameObject interactBackground;

    private int score = 0;
    private Coroutine messageCoroutine;
    private bool hudVisible = true;
    private bool interactionPromptVisible;
    private string currentInteractionPrompt;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        AutoBindHealthBar();
        AutoBindScoreImage();
    }

    void Start()
    {
        AutoBindHealthBar();
        AutoBindScoreImage();
        if (interactPromptText != null) interactPromptText.gameObject.SetActive(false);
        if (interactBackground != null) interactBackground.SetActive(false);
        if (checkpointText != null) checkpointText.gameObject.SetActive(false);
        if (messageBackground != null) messageBackground.SetActive(false);
        ShowHUD(SceneManager.GetActiveScene().name != "MainMenu");
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AutoBindHealthBar();
        AutoBindScoreImage();
        ShowHUD(scene.name != "MainMenu");

        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
            UpdateHealth(playerHealth.currentHealth, playerHealth.maxHealth);
    }

    public void ShowHUD(bool show)
    {
        hudVisible = show;

        SetUIActive(healthBar != null ? healthBar.gameObject : null, show);
        SetUIActive(hpFill != null && hpFill.transform.parent != null ? hpFill.transform.parent.gameObject : null, show);
        SetUIActive(scoreImage != null ? scoreImage.gameObject : null, show);
        SetUIActive(scoreText != null ? scoreText.gameObject : null, show);

        if (!show)
        {
            if (checkpointText != null) checkpointText.gameObject.SetActive(false);
            if (messageBackground != null) messageBackground.SetActive(false);
            HideInteractPrompt();
        }
    }

    public void UpdateHealth(float current, float max)
    {
        AutoBindHealthBar();
        if (max <= 0f) return;

        float ratio = Mathf.Clamp01(current / max);
        if (hpFill != null) hpFill.fillAmount = ratio;
        if (healthBar != null)
        {
            healthBar.maxValue = 1f;
            healthBar.value = ratio;
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public int GetScore() { return score; }

    public void SetScore(int value)
    {
        score = value;
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void ShowInteractPrompt(string message)
    {
        if (interactPromptText != null && currentInteractionPrompt != message)
            interactPromptText.text = message;

        currentInteractionPrompt = message;

        if (interactPromptText != null && !interactPromptText.gameObject.activeSelf)
            interactPromptText.gameObject.SetActive(true);

        if (interactBackground != null && !interactBackground.activeSelf)
            interactBackground.SetActive(true);

        interactionPromptVisible = true;
    }

    public void ShowInteractionPrompt(string message)
    {
        ShowInteractPrompt(message);
    }

    public void HideInteractPrompt()
    {
        if (!interactionPromptVisible)
            return;

        if (interactPromptText != null && interactPromptText.gameObject.activeSelf)
            interactPromptText.gameObject.SetActive(false);
        if (interactBackground != null && interactBackground.activeSelf)
            interactBackground.SetActive(false);

        interactionPromptVisible = false;
        currentInteractionPrompt = null;
    }

    public void HideInteractionPrompt()
    {
        HideInteractPrompt();
    }

    public void ShowCheckpointMessage()
    {
        ShowTimedMessage("Checkpoint Reached!", 2f);
    }

    public void ShowKeyMessage(bool isRedKey)
    {
        ShowTimedMessage(isRedKey ? "Red Key collected!" : "Blue Key collected!", 2f);
    }

    public void ShowTimedMessage(string message, float duration)
    {
        if (checkpointText == null) return;
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(MessageRoutine(message, duration));
    }

    IEnumerator MessageRoutine(string message, float duration)
    {
        if (!hudVisible) yield break;

        checkpointText.text = message;
        checkpointText.gameObject.SetActive(true);
        if (messageBackground != null) messageBackground.SetActive(true);
        yield return new WaitForSeconds(duration);
        checkpointText.gameObject.SetActive(false);
        if (messageBackground != null) messageBackground.SetActive(false);
        messageCoroutine = null;
    }

    void AutoBindHealthBar()
    {
        if (healthBar == null)
        {
            Slider[] sliders = FindObjectsOfType<Slider>(true);
            foreach (Slider slider in sliders)
            {
                if (slider.name == "HealthBar" || slider.name.Contains("Health"))
                {
                    healthBar = slider;
                    break;
                }
            }
        }

        if (hpFill == null)
        {
            Image[] images = FindObjectsOfType<Image>(true);
            foreach (Image image in images)
            {
                if (image.name == "Hp_Fill" || image.name == "HP_Fill" || image.name == "HealthFill")
                {
                    hpFill = image;
                    break;
                }
            }
        }

        if (hpFill == null && healthBar != null && healthBar.fillRect != null)
            hpFill = healthBar.fillRect.GetComponent<Image>();
    }

    void AutoBindScoreImage()
    {
        if (scoreImage != null) return;

        Image[] images = FindObjectsOfType<Image>(true);
        foreach (Image image in images)
        {
            if (image.name == "ScoreImage")
            {
                scoreImage = image;
                break;
            }
        }
    }

    void SetUIActive(GameObject target, bool active)
    {
        if (target != null && target != gameObject)
            target.SetActive(active);
    }
}
