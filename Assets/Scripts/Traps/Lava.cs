using UnityEngine;

public class Lava : MonoBehaviour
{
    [Header("Lava Settings")]
    public bool instantKill = true;
    public float damage = 999f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                if (instantKill)
                    health.InstantKill();
                else
                    health.TakeDamage(damage);
            }
        }
    }
}