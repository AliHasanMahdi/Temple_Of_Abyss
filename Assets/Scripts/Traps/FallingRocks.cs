using UnityEngine;

public class FallingRocks : MonoBehaviour
{
    public GameObject rockPrefab;
    public float spawnInterval = 1.5f;  // how often a rock falls
    public float damage = 40f;
    public float spawnRadius = 2f;      // random spread of rocks

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnRock();
            timer = 0f;
        }
    }

    void SpawnRock()
    {
        // Spawn rock at random position within radius
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnRadius, spawnRadius),
            0f,
            Random.Range(-spawnRadius, spawnRadius)
        );

        Vector3 spawnPosition = transform.position + randomOffset;
        GameObject rock = Instantiate(rockPrefab, spawnPosition, Random.rotation);

        // Destroy rock after 5 seconds so scene doesn't fill up
        Destroy(rock, 5f);
    }
}

// Attach this separately to the rock prefab itself
public class RockDamage : MonoBehaviour
{
    public float damage = 40f;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damage);
        }
    }
}
