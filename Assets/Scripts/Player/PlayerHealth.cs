using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHUD();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHUD();

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHUD();
    }

    void UpdateHUD()
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateHealth(currentHealth, maxHealth);
        else
            Debug.LogWarning("HUDManager not found in scene!");
    }

    public void InstantKill()
    {
        currentHealth = 0;

        // Update HUD to show 0 health
        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateHealth(0, maxHealth);

        Die();
    }

    void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
