using UnityEngine;

public class DamageZone : MonoBehaviour
{
    public float damage = 50f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health =
                other.GetComponent<PlayerHealth>() ??
                other.GetComponentInParent<PlayerHealth>() ??
                other.GetComponentInChildren<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
        EnemyHealth enemy =
           other.GetComponent<EnemyHealth>() ??
           other.GetComponentInParent<EnemyHealth>() ??
           other.GetComponentInChildren<EnemyHealth>();

        if (enemy != null && !enemy.IsDead())
            enemy.TakeDamage(enemy.maxHealth * 2f);

    }
}
