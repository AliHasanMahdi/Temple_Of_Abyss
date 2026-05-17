using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    public Image hpFill;
    public TMP_Text scoreText;
    public TMP_Text checkpointText;
    public TMP_Text interactPromptText;

    private int score = 0;
    private Coroutine messageCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (interactPromptText != null)
            interactPromptText.gameObject.SetActive(false);

        if (checkpointText != null)
            checkpointText.gameObject.SetActive(false);
    }

    public void UpdateHealth(float current, float max)
    {
        if (hpFill != null)
            hpFill.fillAmount = current / max;
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
        if (interactPromptText != null)
        {
            interactPromptText.text = message;
            interactPromptText.gameObject.SetActive(true);
        }
    }

    public void HideInteractPrompt()
    {
        if (interactPromptText != null)
            interactPromptText.gameObject.SetActive(false);
    }

    // Called by Checkpoint.cs
    public void ShowCheckpointMessage()
    {
        ShowTimedMessage("Checkpoint Reached!", 2f);
    }

    // Called by AN_DoorKey.cs
    public void ShowKeyMessage(bool isRedKey)
    {
        ShowTimedMessage(isRedKey ? "Red Key collected!" : "Blue Key collected!", 2f);
    }

    // General timed message — cancels previous if still showing
    public void ShowTimedMessage(string message, float duration)
    {
        if (checkpointText == null) return;
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(MessageRoutine(message, duration));
    }

    IEnumerator MessageRoutine(string message, float duration)
    {
        checkpointText.text = message;
        checkpointText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        checkpointText.gameObject.SetActive(false);
        messageCoroutine = null;
    }
}