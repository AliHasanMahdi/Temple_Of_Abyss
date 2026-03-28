using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    public Slider healthBar;
    public TMP_Text scoreText;
    public TMP_Text checkpointText;

    private int score = 0;

    void Awake()
    {
        Instance = this;
    }

    public void UpdateHealth(float current, float max)
    {
        healthBar.value = (current / max) * 100f;
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score;
    }

    public int GetScore()
    {
        return score;
    }

    public void ShowCheckpointMessage()
    {
        StartCoroutine(ShowMessage());
    }

    System.Collections.IEnumerator ShowMessage()
    {
        checkpointText.text = "Checkpoint Reached!";
        checkpointText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        checkpointText.gameObject.SetActive(false);
    }
}
