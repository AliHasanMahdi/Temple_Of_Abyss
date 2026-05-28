using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Debug = UnityEngine.Debug;

public class DemonAI : MonoBehaviour
{
    [Header("Components")]
    public NavMeshAgent agent;
    public Transform player;
    public Animator anim;

    [Header("Warning")]
    public float warningDuration = 15f;
    private float warningTimer;
    private bool isWarning = true;

    [Header("Combat")]
    public float sightRange = 100f;
    public float attackRange = 15f;
    public float attackCooldown = 1.2f;
    private float nextAttackTime = 0f;
    public LayerMask playerLayer;

    [Header("Memory")]
    public float memoryDuration = 3f;
    private Vector3 lastKnownPlayerPos;
    private float lastSeenTime = -10f;

    [Header("Jump")]
    private bool isJumping = false;
    public float jumpDuration = 0.6f;   // match your jump animation length
    public float jumpHeight = 0.8f;     // arc height

    // Smooth run animation
    private bool _wasRunning = false;
    private float _runGraceTimer = 0f;
    private float _runGracePeriod = 0.3f;

    // Attack state
    private bool isAttacking = false;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        if (anim == null)
            anim = GetComponent<Animator>();

        // Disable auto-traverse so we manually control jumps
        agent.autoTraverseOffMeshLink = false;

        warningTimer = warningDuration;
        agent.isStopped = true;
        agent.updateRotation = true;
        Debug.Log("Demon AI started. Warning for " + warningDuration + " seconds.");
    }

    void Update()
    {
        // ----- WARNING PHASE -----
        if (isWarning)
        {
            warningTimer -= Time.deltaTime;
            if (warningTimer <= 0f)
            {
                isWarning = false;
                agent.isStopped = false;
                Debug.Log("DEMON HUNTING MODE ACTIVATED");
            }
            else
            {
                anim.SetBool("IsRunning", false);
                return;
            }
        }

        // ----- JUMP DETECTION -----
        if (agent.isOnOffMeshLink && !isJumping && !isWarning && !isAttacking)
        {
            StartCoroutine(JumpOverLink());
            return; // skip movement this frame
        }

        // ----- DETECTION -----
        bool canSeePlayer = Physics.CheckSphere(transform.position, sightRange, playerLayer);
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool isInAttackRange = distanceToPlayer <= attackRange;

        if (canSeePlayer)
        {
            lastKnownPlayerPos = player.position;
            lastSeenTime = Time.time;
        }

        // ----- ATTACK -----
        if (isInAttackRange && Time.time >= nextAttackTime && !isWarning && !isJumping && !isAttacking)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
            return;
        }

        // ----- CHASE / IDLE -----
        if (!isWarning && !isJumping && !isAttacking)
        {
            bool hasValidTarget = (canSeePlayer || (Time.time - lastSeenTime) <= memoryDuration);
            if (hasValidTarget)
            {
                Vector3 targetPos = canSeePlayer ? player.position : lastKnownPlayerPos;
                agent.SetDestination(targetPos);
                agent.isStopped = false;

                // Determine if demon should be running
                bool hasActivePath = agent.hasPath && agent.remainingDistance > agent.stoppingDistance;
                bool isMovingFast = agent.velocity.magnitude > 0.2f;
                bool shouldRun = hasActivePath || isMovingFast;

                // Grace timer to prevent flickering
                if (shouldRun)
                {
                    _runGraceTimer = 0f;
                    _wasRunning = true;
                }
                else if (_wasRunning)
                {
                    _runGraceTimer += Time.deltaTime;
                    if (_runGraceTimer < _runGracePeriod)
                        shouldRun = true;
                    else
                        _wasRunning = false;
                }

                anim.SetBool("IsRunning", shouldRun);
            }
            else
            {
                agent.ResetPath();
                agent.isStopped = true;
                anim.SetBool("IsRunning", false);
                _wasRunning = false;
                _runGraceTimer = 0f;
            }
        }
    }

    void Attack()
    {
        isAttacking = true;
        agent.isStopped = true;
        anim.SetTrigger("Attack");
        Debug.Log("DEMON ATTACKS!");

        // Optional damage
        // PlayerHealth health = player.GetComponent<PlayerHealth>();
        // if (health != null) health.TakeDamage(15);

        Invoke(nameof(ResumeAfterAttack), 0.8f);
    }

    void ResumeAfterAttack()
    {
        isAttacking = false;
        agent.isStopped = false;
    }

    // Called by the cutscene to make the demon start hunting immediately
    public void StartHunting()
    {
        if (isWarning)
        {
            isWarning = false;
            warningTimer = 0f;
            agent.isStopped = false;
            Debug.Log("Demon hunting triggered externally - warning skipped.");
        }
    }

    IEnumerator JumpOverLink()
    {
        isJumping = true;
        anim.SetTrigger("Jump");
        Debug.Log("Demon jumps across gap!");

        // Get the link data
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPos = transform.position;
        Vector3 endPos = linkData.endPos;

        // Animate the jump with an arc
        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            float t = elapsed / jumpDuration;
            Vector3 newPos = Vector3.Lerp(startPos, endPos, t);
            // Add an arc: sin wave from 0 to 1 and back
            float arc = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            newPos.y += arc;
            transform.position = newPos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to exact end position
        transform.position = endPos;

        // Tell the agent the link is completed
        agent.CompleteOffMeshLink();

        // Small delay to ensure landing animation can play
        yield return new WaitForSeconds(0.2f);
        isJumping = false;

        // Resume running animation if still chasing
        bool shouldRun = agent.velocity.magnitude > 0.2f || agent.hasPath;
        anim.SetBool("IsRunning", shouldRun);
        Debug.Log("Jump completed");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}