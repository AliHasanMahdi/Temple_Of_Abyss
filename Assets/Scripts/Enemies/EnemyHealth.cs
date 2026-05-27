using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Death")]
    public float destroyDelay = 3f;

    [Header("Room Reference")]
    [Tooltip("Drag the EnemyRoom that owns this enemy here")]
    public EnemyRoom enemyRoom;

    [Header("Audio")]
    public AudioClip deathSound;
    [Range(0f, 1f)]
    public float deathVolume = 0.9f;

    private Animator anim;
    private EnemyAI ai;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponentInChildren<Animator>();
        ai = GetComponent<EnemyAI>();

        // Auto-find room if not assigned in Inspector
        if (enemyRoom == null)
            enemyRoom = GetComponentInParent<EnemyRoom>();

        LoadDefaultSound();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Stop AI
        if (ai != null) ai.enabled = false;

        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // Death animation
        LoadDefaultSound();
        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position, TempleAudio.ScaleSfxVolume(deathVolume));

        // Death animation
        if (anim != null)
        {
            anim.SetBool("IsWalking", false);
            anim.SetBool("IsChasing", false);
            anim.SetBool("IsAttacking", false);
            anim.SetBool("IsDead", true);
            anim.CrossFade("Death", 0.1f, 0);
        }

        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Tell the room this enemy died
        if (enemyRoom != null)
            enemyRoom.OnEnemyDied();

        Destroy(gameObject, destroyDelay);
    }

    public bool IsDead() { return isDead; }

    void LoadDefaultSound()
    {
        if (deathSound == null)
            deathSound = TempleAudio.LoadClip("TempleAudio/Enemy/freesound_community-bones-2-88481");
    }
}
