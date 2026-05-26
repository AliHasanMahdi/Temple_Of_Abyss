using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    int score;
    Coroutine messageCoroutine;
    bool hudVisible = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        AutoBindAll();
    }

    void Start()
    {
        AutoBindAll();
        ApplyScoreText();

        if (interactPromptText != null)
            interactPromptText.gameObject.SetActive(false);

        if (interactBackground != null)
            interactBackground.SetActive(false);

        if (checkpointText != null)
            checkpointText.gameObject.SetActive(false);

        if (messageBackground != null)
            messageBackground.SetActive(false);

        string sceneName = SceneManager.GetActiveScene().name;
        ShowHUD(sceneName != "MainMenu" && sceneName != "GameOver");
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
        AutoBindAll();
        ApplyScoreText();
        ShowHUD(scene.name != "MainMenu" && scene.name != "GameOver");

        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
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
            if (checkpointText != null)
                checkpointText.gameObject.SetActive(false);

            if (messageBackground != null)
                messageBackground.SetActive(false);

            HideInteractPrompt();
        }
    }

    public void UpdateHealth(float current, float max)
    {
        AutoBindHealthBar();
        if (max <= 0f)
            return;

        float ratio = Mathf.Clamp01(current / max);
        if (hpFill != null)
            hpFill.fillAmount = ratio;

        if (healthBar != null)
        {
            healthBar.maxValue = 1f;
            healthBar.value = ratio;
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        ApplyScoreText();
    }

    public int GetScore()
    {
        return score;
    }

    public void SetScore(int value)
    {
        score = value;
        ApplyScoreText();
    }

    public void ShowInteractPrompt(string message)
    {
        AutoBindPrompt();
        if (interactPromptText != null)
        {
            interactPromptText.text = message;
            interactPromptText.gameObject.SetActive(!string.IsNullOrEmpty(message) && hudVisible);
        }

        if (interactBackground != null)
            interactBackground.SetActive(!string.IsNullOrEmpty(message) && hudVisible);
    }

    public void HideInteractPrompt()
    {
        if (interactPromptText != null)
            interactPromptText.gameObject.SetActive(false);

        if (interactBackground != null)
            interactBackground.SetActive(false);
    }

    public void ShowInteractionPrompt(string prompt)
    {
        ShowInteractPrompt(prompt);
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
        AutoBindMessages();
        if (checkpointText == null || !hudVisible)
            return;

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(MessageRoutine(message, duration));
    }

    IEnumerator MessageRoutine(string message, float duration)
    {
        checkpointText.text = message;
        checkpointText.gameObject.SetActive(true);

        if (messageBackground != null)
            messageBackground.SetActive(true);

        yield return new WaitForSeconds(duration);

        checkpointText.gameObject.SetActive(false);
        if (messageBackground != null)
            messageBackground.SetActive(false);

        messageCoroutine = null;
    }

    void ApplyScoreText()
    {
        AutoBindScoreRefs();
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    void AutoBindAll()
    {
        AutoBindHealthBar();
        AutoBindScoreRefs();
        AutoBindMessages();
        AutoBindPrompt();
    }

    void AutoBindHealthBar()
    {
        if (healthBar == null)
        {
            Slider[] sliders = FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Slider slider in sliders)
            {
                if (slider.name == "HealthBar" || slider.name.Contains("Health"))
                {
                    healthBar = slider;
                    break;
                }
            }
        }

        if (hpFill == null && healthBar != null && healthBar.fillRect != null)
            hpFill = healthBar.fillRect.GetComponent<Image>();

        if (hpFill == null)
        {
            Image[] images = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Image image in images)
            {
                if (image.name == "Hp_Fill" || image.name == "HP_Fill" || image.name == "HealthFill")
                {
                    hpFill = image;
                    break;
                }
            }
        }
    }

    void AutoBindScoreRefs()
    {
        if (scoreText == null)
        {
            TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TMP_Text text in texts)
            {
                if (text.name == "ScoreText")
                {
                    scoreText = text;
                    break;
                }
            }
        }

        if (scoreImage == null)
        {
            Image[] images = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Image image in images)
            {
                if (image.name == "ScoreImage")
                {
                    scoreImage = image;
                    break;
                }
            }
        }
    }

    void AutoBindMessages()
    {
        if (checkpointText == null)
        {
            TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TMP_Text text in texts)
            {
                if (text.name == "CheckpointText")
                {
                    checkpointText = text;
                    break;
                }
            }
        }

        if (messageBackground == null)
        {
            GameObject found = GameObject.Find("MessageBackground");
            if (found != null)
                messageBackground = found;
        }
    }

    void AutoBindPrompt()
    {
        if (interactPromptText == null)
        {
            TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TMP_Text text in texts)
            {
                if (text.name == "InteractionPromptText" || text.name == "InteractPromptText")
                {
                    interactPromptText = text;
                    break;
                }
            }
        }

        if (interactBackground == null)
        {
            GameObject found = GameObject.Find("InteractBackground");
            if (found != null)
                interactBackground = found;
        }
    }

    void SetUIActive(GameObject target, bool active)
    {
        if (target != null && target != gameObject)
            target.SetActive(active);
    }
}
