using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public bool IsDead { get; private set; }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHUD();
    }

    public void TakeDamage(float amount)
    {
        if (IsDead)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHUD();

        if (currentHealth <= 0)
            StartCoroutine(Die());
    }

    public void Heal(float amount)
    {
        if (IsDead)
            return;

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

    IEnumerator Die()
    {
        IsDead = true;

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        PlayerInteraction interaction = GetComponent<PlayerInteraction>();
        if (interaction != null)
            interaction.enabled = false;

        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
