using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AIControllerScript : MonoBehaviour
{
    public enum AIState { Patrol, Aggro, Scared, Hiding, Attack }

    [Header("References")]
    public Transform player;
    private NavMeshAgent agent;

    [Header("AI Settings")]
    public AIState currentState = AIState.Patrol;
    public float detectionRange = 15f;
    public float attackRange = 3f;
    public float stoppingDistance = 1.2f;
    public float chaseSpeed = 3.5f;
    public float fleeSpeed = 5f;
    public float patrolSpeed = 1.5f;
    public float playerSpeedThreshold = 3.0f;

    public Animator snake;
    public Collider snakeCollider; // assign in inspector

    [Header("Behavior Variations")]
    public bool aggressive = true;    // will attack player
    public bool timid = false;        // will flee/hide
    public bool wanderer = true;      // will patrol/wander
    public float hideChance = 0.2f;   // chance to hide when scared

    [Header("Patrol")]
    public Transform[] patrolPoints;
    private int patrolIndex = 0;
    private float patrolWaitTimer;
    public float patrolWaitTime = 2f;

    [Header("Safety")]
    public float wallCheckDistance = 1f;
    public float edgeCheckDistance = 1.2f;
    public LayerMask groundMask;
    public LayerMask wallMask;

    [Header("Hiding")]
    public float hideDuration = 5f;
    public float hideCooldown = 5f;
    private float hideCooldownTimer;
    private float hideTimer;
    private float nextHideTime;           // random timer for auto-hiding


    [Header("Attack Settings")]
    public float attackCooldown = 1.2f;
    private float attackCooldownTimer;
    public float lungeDuration = 0.25f;
    private float lungeTimer;
    public float attackSpeedMultiplier = 6f;

    [Header("Rotation Smoothing")]
    public float turnSpeed = 8f;
    public float minMoveToRotate = 0.05f;

    [Header("Unstuck Settings")]
    public float unstuckThreshold = 0.03f;    // how small movement counts as "not moving"
    public float unstuckTime = 1.0f;          // how long before we try to unstuck
    public float unstuckSearchRadius = 2f;    // search radius when picking new target
    private Vector3 lastFramePos;
    private float stuckTimer;

    [Header("Path Updating")]
    public float pathUpdateInterval = 0.25f;
    private float pathUpdateTimer;

    private Vector3 lastPlayerPos;
    private float playerSpeed;
    private Quaternion targetRotation;
    private bool isLunging = false;
    public bool isBurrowed = false;

    //for object pool: snakeObject.GetComponent<AIControllerScript>().ResetOnSpawn();

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // make sure agent settings are sane (you can override in inspector)
        agent.stoppingDistance = stoppingDistance;
        agent.updateRotation = false;   // we rotate manually
        agent.autoBraking = false;
        agent.autoRepath = true;

        targetRotation = transform.rotation;
        lastPlayerPos = player != null ? player.position : Vector3.zero;
        lastFramePos = transform.position;

        // set initial random hide time > 10 seconds
        nextHideTime = Random.Range(10f, 20f);
    }

    public void ResetOnSpawn()
    {
        // reset AI core
        currentState = AIState.Patrol;
        
        hideTimer = 0f;
        hideCooldownTimer = 2f;  // cannot hide instantly on spawn

        attackCooldownTimer = 0f;
        lungeTimer = 0f;
        pathUpdateTimer = 0f;
        isLunging = false;
        isBurrowed = false;

        // reset movement
        if (agent != null)
        {
            agent.updatePosition = true;
            agent.isStopped = false;

            // Force re-enable if pooled object disabled agent accidentally
            agent.enabled = true;
        }


        // play the emerge animation if you have one
        BurrowOut();

        // set new random hide timer
        nextHideTime = Random.Range(10f, 20f);
    }

    private void Update()
    {
        if (!agent.isOnNavMesh) return;

        // cooldowns
        attackCooldownTimer = Mathf.Max(0f, attackCooldownTimer - Time.deltaTime);
        hideCooldownTimer = Mathf.Max(0f, hideCooldownTimer - Time.deltaTime);

        // player speed smoothing (used only to detect "scared" state)
        if (player != null)
        {
            playerSpeed = (player.position - lastPlayerPos).magnitude / Mathf.Max(Time.deltaTime, 1e-6f);
            lastPlayerPos = player.position;
        }

        HandleEdgeAvoidance();

        // ------------------------------
        // RANDOM HIDE TIMER
        // ------------------------------
        if (Time.timeSinceLevelLoad >= nextHideTime && !isBurrowed && timid)
        {
            EnterHideState();
            nextHideTime = Time.timeSinceLevelLoad + Random.Range(10f, 25f); // reset next hide
        }

        if (currentState != AIState.Hiding)
            CheckStateTransitions();

        switch (currentState)
        {
            case AIState.Patrol: Patrol(); break;
            case AIState.Aggro: ChasePlayer(); break;
            case AIState.Scared: TryHideInBushOrFlee(); break;
            case AIState.Hiding: HandleHiding(); break;
            case AIState.Attack: HandleAttack(); break;
        }

        // rotation and unstuck run every frame
        SmoothRotateWithMovement();
        CheckUnstuck();

        // path update timer (used by Chase to periodically refresh destination)
        pathUpdateTimer += Time.deltaTime;
    }

    // ------------------------------
    // PATROL
    // ------------------------------
    void Patrol()
    {
        if (!wanderer) return; // skip patrol if not a wanderer

        agent.speed = patrolSpeed;

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            RandomWanderPatrol();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= stoppingDistance)
        {
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0f)
                GoToNextPatrolPoint();
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        int randomIndex = Random.Range(0, patrolPoints.Length);
        agent.SetDestination(patrolPoints[randomIndex].position);
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        patrolWaitTimer = patrolWaitTime;
    }

    void RandomWanderPatrol()
    {
        if (agent.remainingDistance > 0.5f && agent.hasPath) return;

        Vector3 rnd = Random.insideUnitSphere * 6f + transform.position;
        if (NavMesh.SamplePosition(rnd, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    // ------------------------------
    // CHASE / AGGRO
    // ------------------------------
    void ChasePlayer()
    {
        if (player == null) return;

        agent.speed = chaseSpeed;
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackRange && attackCooldownTimer <= 0f)
        {
            EnterAttackState();
            return;
        }

        // refresh path periodically so the agent updates even if player stands still
        if (pathUpdateTimer >= pathUpdateInterval)
        {
            pathUpdateTimer = 0f;
            agent.SetDestination(player.position);
        }

        // if far away and no path, set destination immediately
        if (!agent.hasPath && dist > stoppingDistance + 0.5f)
            agent.SetDestination(player.position);
    }

    // ------------------------------
    // ATTACK (lunge)
    // ------------------------------
    void EnterAttackState()
    {
        currentState = AIState.Attack;
        lungeTimer = lungeDuration;
        attackCooldownTimer = attackCooldown;

        // temporarily let us move manually without NavMeshAgent correcting position
        isLunging = true;
        agent.isStopped = true;
        agent.updatePosition = false;
    }

    void HandleAttack()
    {
        lungeTimer -= Time.deltaTime;

        if (lungeTimer > 0f)
        {
            // manual movement toward player during lunge
            Vector3 dir = (player.position - transform.position);
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f)
            {
                dir.Normalize();
                transform.position += dir * attackSpeedMultiplier * Time.deltaTime;
            }
            return;
        }

        // restore agent control smoothly
        isLunging = false;
        agent.updatePosition = true;
        agent.isStopped = false;

        // backoff behavior
        StartCoroutine(BackOffRoutine());
        currentState = AIState.Patrol;
    }

    IEnumerator BackOffRoutine()
    {
        currentState = AIState.Scared;

        Vector3 away = (transform.position - player.position).normalized * 4f;
        if (NavMesh.SamplePosition(transform.position + away, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);

        yield return new WaitForSeconds(3f);

        currentState = AIState.Patrol;
    }

    // ------------------------------
    // FLEE / HIDE
    // ------------------------------
    void TryHideInBushOrFlee()
    {
        if (!timid || hideCooldownTimer > 0f) return;

        agent.speed = fleeSpeed;

        Vector3 dir = (transform.position - player.position).normalized;
        Vector3 flee = transform.position + dir * 5f;
        if (NavMesh.SamplePosition(flee, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);

        // chance to hide instead
        if (Random.value < hideChance)
            EnterHideState();
    }

    void EnterHideState()
    {
        if (snakeCollider != null)
            snakeCollider.enabled = false; // make uncatchable

        agent.isStopped = true;            // halt AI movement
        BurrowIn();                        // play burrow animation

        currentState = AIState.Hiding;
        hideTimer = hideDuration;
    }

    void HandleHiding()
    {
        hideTimer -= Time.deltaTime;
        if (hideTimer <= 0f)
            ExitHideState();
    }

    void ExitHideState()
    {
        if (snakeCollider != null)
            snakeCollider.enabled = true; // re-enable collider

        BurrowOut();                       // play emerge animation
        agent.isStopped = false;           // resume AI movement

        currentState = AIState.Patrol;
        hideCooldownTimer = hideCooldown;
    }

    // ------------------------------
    // STATE TRANSITIONS
    // ------------------------------
    void CheckStateTransitions()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < detectionRange)
        {
            // set state based on behavior flags
            if (aggressive)
            {
                currentState = AIState.Aggro;
            }
            else if (timid && Random.value < hideChance)
            {
                currentState = AIState.Scared;
            }
            else if (wanderer)
            {
                currentState = AIState.Patrol;
            }
        }
        else
        {
            if (wanderer)
                currentState = AIState.Patrol;
        }
    }

    // ------------------------------
    // EDGE / WALL HANDLING
    // ------------------------------
    void HandleEdgeAvoidance()
    {
        Vector3 fwd = transform.forward;

        if (Physics.Raycast(transform.position + Vector3.up * 0.3f, fwd, out RaycastHit hitWall, wallCheckDistance, wallMask))
        {
            Vector3 away = (transform.position - hitWall.point).normalized * 1.5f;
            Vector3 candidate = transform.position + away + Random.insideUnitSphere * 1f;
            candidate.y = transform.position.y;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
            else
            {
                Vector3 rnd = transform.position + Quaternion.Euler(0, Random.Range(-120f, 120f), 0) * Vector3.forward * 1.5f;
                if (NavMesh.SamplePosition(rnd, out NavMeshHit hit2, 2f, NavMesh.AllAreas))
                    agent.SetDestination(hit2.position);
            }
        }

        Vector3 downCheckOrigin = transform.position + fwd * 0.3f + Vector3.up * 0.2f;
        if (!Physics.Raycast(downCheckOrigin, Vector3.down, out RaycastHit hitDown, edgeCheckDistance, groundMask))
        {
            Vector3 back = transform.position - fwd * 1.2f + Random.insideUnitSphere * 0.5f;
            back.y = transform.position.y;
            if (NavMesh.SamplePosition(back, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
    }

    // ------------------------------
    // SMOOTH ROTATION
    // ------------------------------
    void SmoothRotateWithMovement()
    {
        if (isLunging && player != null)
        {
            Vector3 dir = player.position - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.0001f)
            {
                targetRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * (turnSpeed * 1.6f));
            }
            return;
        }

        if (agent.velocity.magnitude < minMoveToRotate) return;

        Vector3 moveDir = agent.velocity;
        moveDir.y = 0;
        if (moveDir.sqrMagnitude < 0.0001f) return;

        targetRotation = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
    }

    // ------------------------------
    // UNSTUCK (smooth)
    // ------------------------------
    void CheckUnstuck()
    {
        float moved = Vector3.Distance(transform.position, lastFramePos);

        if (moved < unstuckThreshold)
            stuckTimer += Time.deltaTime;
        else
            stuckTimer = 0f;

        lastFramePos = transform.position;

        if (stuckTimer > unstuckTime)
        {
            bool movedOk = false;
            for (int i = 0; i < 6; i++)
            {
                Vector3 candidate = transform.position + (Quaternion.Euler(0, (i * 60f), 0) * Vector3.forward) * unstuckSearchRadius;
                candidate += Random.insideUnitSphere * 0.5f;
                candidate.y = transform.position.y;

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, unstuckSearchRadius, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                    movedOk = true;
                    break;
                }
            }

            if (!movedOk)
            {
                Vector3 rnd = transform.position + Random.insideUnitSphere * 1.5f;
                rnd.y = transform.position.y;
                if (NavMesh.SamplePosition(rnd, out NavMeshHit hit2, 2f, NavMesh.AllAreas))
                    agent.SetDestination(hit2.position);
            }

            stuckTimer = 0f;
        }
    }

    // ------------------------------
    // BURROW placeholders
    // ------------------------------
    void BurrowIn()
    {
        snake.Play("BurrowIn");
        isBurrowed = true;
        // TODO: animate burrow
        Debug.Log("Burrowing...");
    }

    void BurrowOut()
    {
        snake.Play("BurrowOut");
        isBurrowed = false;
        // TODO: animate emerge
        Debug.Log("Emerging...");
    }
}
