using UnityEngine;

public class SpearTrap : MonoBehaviour
{
    [Header("Spear Settings")]
    public GameObject spearPrefab;
    public float shootInterval = 3f;
    public float spearSpeed = 15f;
    public float spearDamage = 40f;
    public float spearLifetime = 4f;

    [Header("Direction")]
    public Vector3 shootDirection = Vector3.forward;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= shootInterval)
        {
            timer = 0f;
            ShootSpear();
        }
    }

    void ShootSpear()
    {
        if (spearPrefab == null) return;

        // Spawn spear at launcher position
        GameObject spear = Instantiate(
            spearPrefab,
            transform.position,
            transform.rotation
        );

        // Add rigidbody for physics movement
        Rigidbody rb = spear.GetComponent<Rigidbody>();
        if (rb == null)
            rb = spear.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.linearVelocity = transform.TransformDirection(shootDirection.normalized) * spearSpeed;

        // Add damage script to spear
        SpearDamage dmg = spear.GetComponent<SpearDamage>();
        if (dmg == null)
            dmg = spear.AddComponent<SpearDamage>();

        dmg.damage = spearDamage;

        // Destroy after lifetime
        Destroy(spear, spearLifetime);
    }

    // Draw shoot direction in scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position,
            transform.TransformDirection(shootDirection.normalized) * 5f);
    }
}