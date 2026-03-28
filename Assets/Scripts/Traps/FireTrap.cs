using UnityEngine;

public class FireTrap : MonoBehaviour
{
    public float damage = 30f;          // damage per second
    public float activeTime = 3f;       // how long fire stays on
    public float cooldownTime = 2f;     // how long fire stays off
    public ParticleSystem fireParticles;

    private bool isActive = true;
    private float timer = 0f;

    void Start()
    {
        if (fireParticles != null)
            fireParticles.Play();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (isActive && timer >= activeTime)
        {
            // Turn fire off
            isActive = false;
            timer = 0f;
            if (fireParticles != null)
                fireParticles.Stop();
        }
        else if (!isActive && timer >= cooldownTime)
        {
            // Turn fire on
            isActive = true;
            timer = 0f;
            if (fireParticles != null)
                fireParticles.Play();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (isActive && other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damage * Time.deltaTime);
        }
    }
}
