using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public float damage = 50f;
    public float riseSpeed = 2f;
    public float waitTime = 1.5f;

    private Vector3 downPosition;
    private Vector3 upPosition;
    private bool rising = true;
    private float timer = 0f;

    void Start()
    {
        downPosition = transform.position;
        upPosition = new Vector3(transform.position.x,
                                 transform.position.y + 1f,
                                 transform.position.z);
    }

    void Update()
    {
        if (rising)
        {
            // Move spikes up
            transform.position = Vector3.MoveTowards(transform.position,
                                                     upPosition,
                                                     riseSpeed * Time.deltaTime);
            if (transform.position == upPosition)
            {
                rising = false;
                timer = 0f;
            }
        }
        else
        {
            // Wait then go back down
            timer += Time.deltaTime;
            if (timer >= waitTime)
            {
                transform.position = Vector3.MoveTowards(transform.position,
                                                         downPosition,
                                                         riseSpeed * Time.deltaTime);
                if (transform.position == downPosition)
                    rising = true;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Damage the player
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damage);
        }
    }
}