using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

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
        if (!PlayerPrefs.HasKey("SavedPosX")) yield break;

        float x = PlayerPrefs.GetFloat("SavedPosX", 0f);
        float y = PlayerPrefs.GetFloat("SavedPosY", 1f);
        float z = PlayerPrefs.GetFloat("SavedPosZ", 0f);

        Vector3 savedPos = new Vector3(x, y, z);
        Debug.Log("Restoring to: " + savedPos);

        CharacterController cc = GetComponent<CharacterController>();

        // Disable → move → wait → enable
        if (cc != null) cc.enabled = false;
        transform.position = savedPos;
        yield return null;
        yield return null;
        if (cc != null) cc.enabled = true;
        yield return null;

        Debug.Log("CharacterController enabled: " + cc.enabled);

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
        ShouldRestorePosition = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}