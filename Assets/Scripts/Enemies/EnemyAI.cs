using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public float waitAtPointTime = 2f;

    [Header("Chase Settings")]
    public float chaseSpeed = 5f;
    public float detectionRange = 8f;
    public float damageRange = 1.5f;
    public float damage = 10f;
    public float damageInterval = 1f;

    [Header("Room Settings")]
    public Collider roomBoundary;

    private enum State { Patrolling, Chasing, Returning, Dead }
    private State currentState = State.Patrolling;

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private float damageTimer = 0f;
    private bool playerInRoom = false;
    private bool isDead = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (isDead) return;

        // Check if player is inside room boundary
        if (roomBoundary != null)
            playerInRoom = roomBoundary.bounds.Contains(player.position);

        switch (currentState)
        {
            case State.Patrolling:
                HandlePatrol();
                LookForPlayer();
                break;

            case State.Chasing:
                HandleChase();
                break;

            case State.Returning:
                HandleReturn();
                break;
        }

        UpdateAnimations();
    }

    // ── PATROL ──────────────────────────────
    void HandlePatrol()
    {
        agent.speed = patrolSpeed;

        if (patrolPoints.Length == 0) return;

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitAtPointTime)
            {
                isWaiting = false;
                waitTimer = 0f;
                GoToNextPatrolPoint();
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            isWaiting = true;
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void LookForPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (playerInRoom && distanceToPlayer <= detectionRange)
        {
            currentState = State.Chasing;

            if (HUDManager.Instance != null)
                HUDManager.Instance.ShowInteractPrompt("Enemy spotted you!");

            Invoke("HidePrompt", 2f);
        }
    }

    // ── CHASE ──────────────────────────────
    void HandleChase()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= damageRange)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                TriggerAttack();
            }
        }

        // Player left the room — return to patrol
        if (!playerInRoom)
        {
            currentState = State.Returning;
            ReturnToPatrol();

            if (HUDManager.Instance != null)
                HUDManager.Instance.HideInteractPrompt();
        }
    }

    void TriggerAttack()
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);

        // Trigger attack animation
        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
            Invoke("StopAttack", 0.8f);
        }
    }

    void StopAttack()
    {
        if (animator != null)
            animator.SetBool("IsAttacking", false);
    }

    // ── RETURN ──────────────────────────────
    void HandleReturn()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = State.Patrolling;
            GoToNextPatrolPoint();
        }
    }

    void ReturnToPatrol()
    {
        float closestDist = Mathf.Infinity;
        int closestIndex = 0;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, patrolPoints[i].position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        currentPatrolIndex = closestIndex;
        agent.SetDestination(patrolPoints[closestIndex].position);
    }

    // ── ANIMATIONS ──────────────────────────
    void UpdateAnimations()
    {
        if (animator == null) return;

        bool isMoving = agent.velocity.magnitude > 0.1f;
        bool isChasing = currentState == State.Chasing;

        animator.SetBool("IsWalking", isMoving && !isChasing);
        animator.SetBool("IsChasing", isChasing);
    }

    // ── DEATH ──────────────────────────────
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        currentState = State.Dead;

        agent.isStopped = true;

        if (animator != null)
            animator.SetBool("IsDead", true);

        // Destroy enemy after death animation
        Destroy(gameObject, 3f);
    }

    void HidePrompt()
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.HideInteractPrompt();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRange);
    }
}