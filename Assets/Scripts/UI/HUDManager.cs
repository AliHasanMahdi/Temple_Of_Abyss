using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    public Slider healthBar;
    public TMP_Text scoreText;
    public TMP_Text checkpointText;
    public TMP_Text interactPromptText;

    private int score = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Hide prompt at start
        if (interactPromptText != null)
            interactPromptText.gameObject.SetActive(false);
    }

    public void UpdateHealth(float current, float max)
    {
        if (healthBar != null)
            healthBar.value = (current / max) * 100f;
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public int GetScore()
    {
        return score;
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

    public void ShowCheckpointMessage()
    {
        StartCoroutine(ShowMessage());
    }

    System.Collections.IEnumerator ShowMessage()
    {
        if (checkpointText != null)
        {
            checkpointText.text = "Checkpoint Reached!";
            checkpointText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            checkpointText.gameObject.SetActive(false);
        }
    }
}