using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public float waitAtPointTime = 2f;

    [Header("Vision Settings")]
    public Camera enemyCamera;
    public float visionRange = 12f;
    public float fieldOfView = 90f;
    public LayerMask obstacleMask;

    [Header("Chase Settings")]
    public float chaseSpeed = 5f;
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

    // Vision
    private bool canSeePlayer = false;
    private float lostSightTimer = 0f;
    public float lostSightDelay = 3f; // seconds before giving up chase

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Auto find camera if not assigned
        if (enemyCamera == null)
            enemyCamera = GetComponentInChildren<Camera>();

        // Disable enemy camera from rendering
        // We only use it for vision checks not actual rendering
        if (enemyCamera != null)
            enemyCamera.enabled = false;

        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (isDead) return;

        if (player != null)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            float dist = directionToPlayer.magnitude;
            float angle = Vector3.Angle(transform.forward, directionToPlayer);

            RaycastHit hit;
            Vector3 eyePos = transform.position + Vector3.up * 1.7f;
            bool rayHit = Physics.Raycast(eyePos, directionToPlayer.normalized, out hit, visionRange);
            string hitName = rayHit ? hit.transform.name : "Nothing";

            Debug.Log("Dist: " + dist.ToString("F1")
                    + " | Angle: " + angle.ToString("F1")
                    + " | RayHit: " + hitName
                    + " | State: " + currentState
                    + " | CanSee: " + canSeePlayer);
        }

        // Check room boundary
        if (roomBoundary != null)
            playerInRoom = roomBoundary.bounds.Contains(player.position);

        // Always check vision
        canSeePlayer = CheckVision();

        switch (currentState)
        {
            case State.Patrolling:
                HandlePatrol();
                if (canSeePlayer) StartChasing();
                break;

            case State.Chasing:
                HandleChase();
                break;

            case State.Returning:
                HandleReturn();
                if (canSeePlayer) StartChasing();
                break;
        }

        UpdateAnimations();
    }

    // ── VISION ──────────────────────────────

    bool CheckVision()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float dist = directionToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (dist > visionRange) return false;
        if (angle > fieldOfView / 2f) return false;

        Vector3 eyePos = transform.position + Vector3.up * 1.7f;
        Vector3 playerChest = player.position + Vector3.up * 1f;
        Vector3 dirToChest = (playerChest - eyePos).normalized;

        // Ignore the enemy's own collider
        int enemyLayer = LayerMask.GetMask("Enemy");
        int ignoreEnemyMask = ~enemyLayer;

        RaycastHit hit;
        if (Physics.Raycast(eyePos, dirToChest, out hit, visionRange, ignoreEnemyMask))
        {
            if (hit.transform.CompareTag("Player"))
                return true;
            else
                return false;
        }

        return true;
    }

    void StartChasing()
    {
        currentState = State.Chasing;
        lostSightTimer = 0f;

        if (HUDManager.Instance != null)
            HUDManager.Instance.ShowInteractPrompt("Enemy spotted you!");

        Invoke("HidePrompt", 2f);
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

    // ── CHASE ──────────────────────────────
    void HandleChase()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        // Deal damage when close enough
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > damageRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            // Stop moving and attack
            agent.isStopped = true;

            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                TriggerAttack();
            }
        }

        // Lost sight of player
        if (!canSeePlayer)
        {
            lostSightTimer += Time.deltaTime;

            // Keep chasing for lostSightDelay seconds
            // then give up if player left room
            if (lostSightTimer >= lostSightDelay)
            {
                if (!playerInRoom)
                {
                    // Player left room — return to patrol
                    currentState = State.Returning;
                    ReturnToPatrol();

                    if (HUDManager.Instance != null)
                        HUDManager.Instance.HideInteractPrompt();
                }
                else
                {
                    // Player still in room but hidden
                    // Keep searching — go to last known position
                    agent.SetDestination(player.position);
                }
            }
        }
        else
        {
            // Reset lost sight timer when player visible again
            lostSightTimer = 0f;
        }
    }

    void TriggerAttack()
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);

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
            float dist = Vector3.Distance(transform.position,
                                          patrolPoints[i].position);
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

        Destroy(gameObject, 3f);
    }

    void HidePrompt()
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.HideInteractPrompt();
    }

    // Draw vision cone in Scene view
    void OnDrawGizmosSelected()
    {
        // Vision range — yellow
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Damage range — red
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRange);

        // Vision cone — cyan lines
        Gizmos.color = Color.cyan;
        Vector3 eyePos = transform.position + Vector3.up * 1.7f;
        float halfFOV = fieldOfView / 2f;

        Vector3 leftDir = Quaternion.Euler(0, -halfFOV, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, halfFOV, 0) * transform.forward;

        Gizmos.DrawLine(eyePos, eyePos + leftDir * visionRange);
        Gizmos.DrawLine(eyePos, eyePos + rightDir * visionRange);
        Gizmos.DrawLine(eyePos, eyePos + transform.forward * visionRange);
    }
}