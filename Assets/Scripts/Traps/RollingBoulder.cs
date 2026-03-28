using UnityEngine;

public class RollingBoulder : MonoBehaviour
{
    public float rollSpeed = 8f;
    public float damage = 100f;
    public Vector3 rollDirection = Vector3.forward;
    public float triggerDistance = 5f;

    private Rigidbody rb;
    private bool isRolling = false;
    private Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // stays still until triggered
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (!isRolling && player != null)
        {
            // Start rolling when player gets close
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= triggerDistance)
            {
                isRolling = true;
                rb.isKinematic = false;
                rb.AddForce(rollDirection * rollSpeed, ForceMode.Impulse);
            }
        }
    }

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