using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Audio")]
    public AudioSource playerAudioSource;
    public AudioClip damageSound;

    public static bool ShouldRestorePosition = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHUD();

        if (ShouldRestorePosition)
        {
            ShouldRestorePosition = false;
            StartCoroutine(RestoreAfterLoad());
        }
    }

    IEnumerator RestoreAfterLoad()
    {
        yield return null;
        yield return null;

        string savedScene = PlayerPrefs.GetString("SavedScene", "");
        string currentScene = SceneManager.GetActiveScene().name;

        if (savedScene != currentScene) yield break;
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.RestorePlayerToSavedPosition(gameObject);
        }
        else
        {
            Vector3 savedPosition = new Vector3(
                PlayerPrefs.GetFloat("SavedPosX", transform.position.x),
                PlayerPrefs.GetFloat("SavedPosY", transform.position.y),
                PlayerPrefs.GetFloat("SavedPosZ", transform.position.z));
            transform.position = SaveSystem.GetSafePlayerPosition(savedPosition, gameObject);
        }

        yield return new WaitForFixedUpdate();
        yield return null;

        // Restore score
        int savedScore = PlayerPrefs.GetInt("SavedScore", 0);
        if (HUDManager.Instance != null)
            HUDManager.Instance.SetScore(savedScore);

        // Restore keys
        AN_HeroInteractive hero = GetComponent<AN_HeroInteractive>();
        if (hero != null)
        {
            hero.RedKey = PlayerPrefs.GetInt("SavedRedKey", 0) == 1;
            hero.BlueKey = PlayerPrefs.GetInt("SavedBlueKey", 0) == 1;
        }

        Debug.Log("Restore complete! Can move now.");
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Play damage sound
        if (playerAudioSource != null && damageSound != null)
        {
            playerAudioSource.PlayOneShot(damageSound);
        }

        UpdateHUD();
        if (currentHealth <= 0) Die();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHUD();
    }

    public void InstantKill()
    {
        currentHealth = 0;
        UpdateHUD();
        Die();
    }

    void UpdateHUD()
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    void Die()
    {
        ShouldRestorePosition = false;
        GameOverMenu.ShowDeath(SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("GameOver");
    }
}