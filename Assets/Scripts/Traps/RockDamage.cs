using UnityEngine;

public class RockDamage : MonoBehaviour
{
    public float damage = 40f;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Try direct
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();

            // Try parent
            if (health == null)
                health = collision.gameObject.GetComponentInParent<PlayerHealth>();

            // Try children
            if (health == null)
                health = collision.gameObject.GetComponentInChildren<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log("Rock dealt " + damage + " damage!");
            }
            else
            {
                Debug.LogWarning("PlayerHealth not found on: " + collision.gameObject.name);
            }
        }
    }
}