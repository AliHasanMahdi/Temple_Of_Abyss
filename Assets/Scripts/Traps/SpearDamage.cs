using UnityEngine;

public class SpearDamage : MonoBehaviour
{
    public float damage = 40f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>()
                               ?? other.GetComponentInParent<PlayerHealth>()
                               ?? other.GetComponentInChildren<PlayerHealth>()
                               ?? FindObjectOfType<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log("Spear hit player for " + damage + " damage!");
            }

            // Destroy spear on player hit
            Destroy(gameObject);
        }

        // Destroy spear when hitting walls
        else if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}