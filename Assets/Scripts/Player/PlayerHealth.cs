using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public bool IsDead { get; private set; }
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

        string savedScene = PlayerPrefs.GetString("SavedScene", string.Empty);
        string currentScene = SceneManager.GetActiveScene().name;

        if (savedScene != currentScene)
            yield break;

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

        int savedScore = PlayerPrefs.GetInt("SavedScore", 0);
        if (HUDManager.Instance != null)
            HUDManager.Instance.SetScore(savedScore);

        AN_HeroInteractive hero = GetComponent<AN_HeroInteractive>();
        if (hero != null)
        {
            int redKeyCount = PlayerPrefs.HasKey("SavedRedKeyCount")
                ? PlayerPrefs.GetInt("SavedRedKeyCount", 0)
                : PlayerPrefs.GetInt("SavedRedKey", 0);
            int blueKeyCount = PlayerPrefs.HasKey("SavedBlueKeyCount")
                ? PlayerPrefs.GetInt("SavedBlueKeyCount", 0)
                : PlayerPrefs.GetInt("SavedBlueKey", 0);

            hero.SetKeyCounts(redKeyCount, blueKeyCount);
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHUD();

        if (currentHealth <= 0f)
            StartCoroutine(Die());
    }

    public void Heal(float amount)
    {
        if (IsDead)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHUD();
    }

    public void InstantKill()
    {
        if (IsDead)
            return;

        currentHealth = 0f;
        UpdateHUD();
        StartCoroutine(Die());
    }

    void UpdateHUD()
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    IEnumerator Die()
    {
        if (IsDead)
            yield break;

        IsDead = true;

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        PlayerInteraction interaction = GetComponent<PlayerInteraction>();
        if (interaction != null)
            interaction.enabled = false;

        yield return new WaitForSeconds(1f);

        ShouldRestorePosition = false;
        GameOverMenu.ShowDeath(SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("GameOver");
    }
}
