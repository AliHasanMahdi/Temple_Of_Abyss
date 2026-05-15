using UnityEngine;

public class SpikeDamage : MonoBehaviour
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
    }
}
