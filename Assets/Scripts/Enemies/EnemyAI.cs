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

    // ── FOOTSTEP SOUNDS ──────────────────────────────────────────
    [Header("Enemy Footstep Sounds")]
    [Tooltip("AudioSource on this enemy used for footsteps (3D)")]
    public AudioSource footstepAudioSource;

    [Tooltip("Footstep clips while patrolling (e.g. Footstep_Boots_01..07)")]
    public AudioClip[] patrolFootstepClips;

    [Tooltip("Footstep clips while chasing (e.g. Footstep_Deep_01..07)")]
    public AudioClip[] chaseFootstepClips;

    [Tooltip("Steps per second while patrolling")]
    public float patrolStepRate = 1.6f;

    [Tooltip("Steps per second while chasing")]
    public float chaseStepRate = 2.4f;

    [Range(0f, 1f)]
    public float footstepVolume = 0.35f;

    [Tooltip("Enemy walk sound only plays when the player is within this distance.")]
    public float footstepAudibleDistance = 7f;
    // ─────────────────────────────────────────────────────────────

    private enum State { Patrolling, Chasing, Returning, Dead }
    private State currentState = State.Patrolling;

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private Rigidbody rb;
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private float damageTimer = 0f;
    private bool playerInRoom = false;
    private bool isDead = false;
    private float nextPlayerSearchTime = 0f;
    private float stuckTimer = 0f;
    private Vector3 lastPosition;

    // Vision
    private bool canSeePlayer = false;
    private float lostSightTimer = 0f;
    public float lostSightDelay = 3f;

    // Footstep timing
    private float footstepTimer = 0f;
    const float MaxFootstepAudibleDistance = 7f;
    const float FootstepVolumeScale = 0.45f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        FindPlayer();

        if (enemyCamera == null)
            enemyCamera = GetComponentInChildren<Camera>();

        if (enemyCamera != null)
        {
            enemyCamera.enabled = false;
            AudioListener enemyListener = enemyCamera.GetComponent<AudioListener>();
            if (enemyListener != null)
                enemyListener.enabled = false;
        }

        if (agent == null)
        {
            Debug.LogWarning("EnemyAI needs a NavMeshAgent component.", this);
            enabled = false;
            return;
        }

        agent.autoRepath = true;
        PrepareRigidbodyForNavMesh();
        PlaceAgentOnNavMesh();
        lastPosition = transform.position;
        GoToNextPatrolPoint();

        // Auto-create/configure 3D AudioSource if not assigned.
        if (footstepAudioSource == null)
            footstepAudioSource = gameObject.AddComponent<AudioSource>();

        ConfigureFootstepAudioSource();

        if (!HasPlayableClips(patrolFootstepClips))
            patrolFootstepClips = TempleAudio.LoadClips("TempleAudio/Enemy/freesound_community-rattling-bones-105394");

        if (!HasPlayableClips(chaseFootstepClips))
            chaseFootstepClips = patrolFootstepClips;
    }

    void PrepareRigidbodyForNavMesh()
    {
        if (rb == null) return;
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Update()
    {
        if (isDead) return;

        if (player == null)
            FindPlayer();

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            PlaceAgentOnNavMesh();
            if (agent == null) return;
            if (!agent.isOnNavMesh) return;
        }

        if (roomBoundary != null && player != null)
            playerInRoom = roomBoundary.bounds.Contains(player.position);
        else
            playerInRoom = false;

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

        RecoverIfStuck();
        UpdateAnimations();
        HandleFootsteps();   // <-- new
    }

    void FindPlayer()
    {
        if (Time.time < nextPlayerSearchTime) return;
        nextPlayerSearchTime = Time.time + 0.5f;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject != null ? playerObject.transform : null;
    }

    void PlaceAgentOnNavMesh()
    {
        if (agent == null) return;
        if (agent.isOnNavMesh) return;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 3f, agent.areaMask))
            agent.Warp(hit.position);
        else
            Debug.LogWarning("Enemy is not on the NavMesh. Move it onto a baked walkable area.", this);
    }

    bool TrySetDestination(Vector3 targetPosition)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return false;

        NavMeshHit hit;
        if (!NavMesh.SamplePosition(targetPosition, out hit, 2f, agent.areaMask))
            return false;

        agent.isStopped = false;
        return agent.SetDestination(hit.position);
    }

    // ── VISION ──────────────────────────────────────────────────

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

        int enemyLayer = LayerMask.GetMask("Enemy");
        int ignoreEnemyMask = ~enemyLayer;

        RaycastHit hit;
        if (Physics.Raycast(eyePos, dirToChest, out hit, visionRange, ignoreEnemyMask))
        {
            return hit.transform.CompareTag("Player");
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

    // ── PATROL ──────────────────────────────────────────────────

    void HandlePatrol()
    {
        agent.speed = patrolSpeed;

        if (patrolPoints == null || patrolPoints.Length == 0) return;

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

        if (!agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathInvalid)
            GoToNextPatrolPoint();
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        currentPatrolIndex %= patrolPoints.Length;

        if (patrolPoints[currentPatrolIndex] == null)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            return;
        }

        TrySetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // ── CHASE ────────────────────────────────────────────────────

    void HandleChase()
    {
        if (player == null)
        {
            currentState = State.Returning;
            ReturnToPatrol();
            return;
        }

        agent.speed = chaseSpeed;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > damageRange)
        {
            TrySetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;

            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                TriggerAttack();
            }
        }

        if (!canSeePlayer)
        {
            lostSightTimer += Time.deltaTime;

            if (lostSightTimer >= lostSightDelay)
            {
                if (!playerInRoom)
                {
                    agent.isStopped = false;
                    currentState = State.Returning;
                    ReturnToPatrol();

                    if (HUDManager.Instance != null)
                        HUDManager.Instance.HideInteractPrompt();
                }
                else
                {
                    TrySetDestination(player.position);
                }
            }
        }
        else
        {
            lostSightTimer = 0f;
        }
    }

    void TriggerAttack()
    {
        if (player == null) return;

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health == null) health = player.GetComponentInParent<PlayerHealth>();
        if (health == null) health = FindObjectOfType<PlayerHealth>();

        if (health != null)
            health.TakeDamage(damage);
        else
            Debug.LogWarning("PlayerHealth not found!");

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

    // ── RETURN ───────────────────────────────────────────────────

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
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        float closestDist = Mathf.Infinity;
        int closestIndex = 0;
        bool foundPoint = false;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;

            float dist = Vector3.Distance(transform.position, patrolPoints[i].position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
                foundPoint = true;
            }
        }

        if (!foundPoint) return;

        currentPatrolIndex = closestIndex;
        TrySetDestination(patrolPoints[closestIndex].position);
    }

    void RecoverIfStuck()
    {
        if (agent == null || !agent.isOnNavMesh || agent.isStopped || isWaiting)
        {
            lastPosition = transform.position;
            stuckTimer = 0f;
            return;
        }

        bool wantsToMove = agent.hasPath && agent.remainingDistance > agent.stoppingDistance + 0.25f;
        bool barelyMoved = Vector3.Distance(transform.position, lastPosition) < 0.02f;

        if (wantsToMove && barelyMoved)
            stuckTimer += Time.deltaTime;
        else
            stuckTimer = 0f;

        lastPosition = transform.position;

        if (stuckTimer < 1.5f) return;

        stuckTimer = 0f;
        if (currentState == State.Chasing && player != null)
            TrySetDestination(player.position);
        else
            GoToNextPatrolPoint();
    }

    // ── ANIMATIONS ───────────────────────────────────────────────

    void UpdateAnimations()
    {
        if (animator == null) return;

        bool isMoving = agent != null && agent.velocity.magnitude > 0.1f;
        bool isChasing = currentState == State.Chasing;

        animator.SetBool("IsWalking", isMoving && !isChasing);
        animator.SetBool("IsChasing", isChasing);
    }

    // ── ENEMY FOOTSTEP SOUND ─────────────────────────────────────
    void HandleFootsteps()
    {
        if (agent == null || footstepAudioSource == null) return;
        if (player == null) return;

        float audibleDistance = EffectiveFootstepAudibleDistance();
        if (Vector3.Distance(transform.position, player.position) > audibleDistance)
        {
            footstepTimer = 0f;
            return;
        }

        float speed = agent.velocity.magnitude;
        if (speed < 0.2f || agent.isStopped)
        {
            footstepTimer = 0f;
            return;
        }

        bool chasing = currentState == State.Chasing;
        float stepRate = chasing ? chaseStepRate : patrolStepRate;
        AudioClip[] clips = (chasing && chaseFootstepClips != null && chaseFootstepClips.Length > 0)
            ? chaseFootstepClips
            : patrolFootstepClips;

        footstepTimer += Time.deltaTime;

        if (footstepTimer >= 1f / stepRate)
        {
            footstepTimer = 0f;
            PlayRandomFootstep(clips);
        }
    }

    void PlayRandomFootstep(AudioClip[] clips)
    {
        if (!HasPlayableClips(clips)) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        for (int i = 0; clip == null && i < clips.Length; i++)
            clip = clips[i];

        footstepAudioSource.pitch = Random.Range(0.90f, 1.10f);
        footstepAudioSource.PlayOneShot(clip, EffectiveFootstepVolume());
    }

    void ConfigureFootstepAudioSource()
    {
        if (footstepAudioSource == null) return;

        footstepAudioSource.spatialBlend = 1f;
        footstepAudioSource.rolloffMode = AudioRolloffMode.Linear;
        footstepAudioSource.minDistance = 0.5f;
        footstepAudioSource.maxDistance = EffectiveFootstepAudibleDistance();
        footstepAudioSource.playOnAwake = false;
    }

    float EffectiveFootstepAudibleDistance()
    {
        return Mathf.Min(footstepAudibleDistance, MaxFootstepAudibleDistance);
    }

    float EffectiveFootstepVolume()
    {
        return TempleAudio.ScaleSfxVolume(footstepVolume * FootstepVolumeScale);
    }

    bool HasPlayableClips(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return false;
        foreach (AudioClip clip in clips)
        {
            if (clip != null) return true;
        }
        return false;
    }
    // ─────────────────────────────────────────────────────────────

    // ── DEATH ────────────────────────────────────────────────────

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        currentState = State.Dead;
        if (agent != null && agent.enabled && agent.isOnNavMesh)
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRange);

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
