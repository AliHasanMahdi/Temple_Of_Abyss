using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    [Header("Scene Settings")]
    public string nextScene = "Level02_Corridor";

    [Header("Optional — require all coins collected")]
    public bool requireAllCoins = false;
    public int totalCoinsInLevel = 0;

    private bool isTransitioning = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
        {
            if (requireAllCoins)
            {
                int collectedCoins = PlayerPrefs.GetInt("CoinsCollected", 0);
                if (collectedCoins < totalCoinsInLevel)
                {
                    if (HUDManager.Instance != null)
                        HUDManager.Instance.ShowInteractPrompt(
                            "Collect all coins first! " + collectedCoins + "/" + totalCoinsInLevel);
                    return;
                }
            }

            isTransitioning = true;
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        // Save progress before loading
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.SaveGame();

        // Reset coins for next level
        PlayerPrefs.SetInt("CoinsCollected", 0);
        PlayerPrefs.Save();

        // Load next scene
        SceneManager.LoadScene(nextScene);
    }
}