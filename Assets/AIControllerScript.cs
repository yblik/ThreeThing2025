using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AIControllerScript : MonoBehaviour
{
    public enum AIState { Patrol, Aggro, Scared, Hiding, Attack }

    [Header("References")]
    public Transform player;
    private NavMeshAgent agent;
    private MeshRenderer[] renderers;

    [Header("AI Settings")]
    public AIState currentState = AIState.Patrol;
    public float detectionRange = 15f;
    public float attackRange = 3f;
    public float stoppingDistance = 1.2f;
    public float chaseSpeed = 3.5f;
    public float fleeSpeed = 5f;
    public float patrolSpeed = 1.5f;
    public float playerSpeedThreshold = 3.0f;

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
    private BushSpot targetBush;

    [Header("Attack Settings")]
    public float attackCooldown = 1.2f;
    private float attackCooldownTimer;
    public float lungeDuration = 0.25f;
    private float lungeTimer;
    public float attackSpeedMultiplier = 6f;

    private Vector3 lastPlayerPos;
    private float playerSpeed;

    private Quaternion targetRotation;


    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        renderers = GetComponentsInChildren<MeshRenderer>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        lastPlayerPos = player.position;

        agent.stoppingDistance = stoppingDistance;

        // IMPORTANT — let us control rotation manually
        agent.updateRotation = false;

        targetRotation = transform.rotation;
    }

    private void Update()
    {
        if (!agent.isOnNavMesh) return;

        // cooldowns
        attackCooldownTimer -= Time.deltaTime;
        hideCooldownTimer -= Time.deltaTime;

        // player speed
        playerSpeed = (player.position - lastPlayerPos).magnitude / Time.deltaTime;
        lastPlayerPos = player.position;

        HandleEdgeAvoidance();

        if (currentState != AIState.Hiding)
            CheckStateTransitions();

        switch (currentState)
        {
            case AIState.Patrol: Patrol(); break;
            case AIState.Aggro: ChasePlayer(); break;
            case AIState.Scared: TryHideInBush(); break;
            case AIState.Hiding: HandleHiding(); break;
            case AIState.Attack: HandleAttack(); break;
        }

        SmoothRotate();
        StabilizeIdle();
    }

    // ------------------------------------------------------
    // ROTATION (ANTI-JITTER)
    // ------------------------------------------------------

    void SmoothRotate()
    {
        Vector3 vel = agent.velocity;

        if (vel.sqrMagnitude > 0.1f)
        {
            Vector3 dir = vel.normalized;
            targetRotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * 7f
        );
    }

    void StabilizeIdle()
    {
        if (agent.velocity.sqrMagnitude < 0.01f)
        {
            agent.angularSpeed = 0;   // freeze Unity's correction rotation
            return;
        }

        agent.angularSpeed = 120; // normal turning
    }

    // ------------------------------------------------------
    // PATROL
    // ------------------------------------------------------

    void Patrol()
    {
        agent.speed = patrolSpeed;

        if (patrolPoints.Length == 0)
        {
            RandomWanderPatrol();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= stoppingDistance)
        {
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0)
                GoToNextPatrolPoint();
        }
    }

    void RandomWanderPatrol()
    {
        if (agent.remainingDistance > 0.5f) return;

        Vector3 rnd = Random.insideUnitSphere * 6f;
        rnd += transform.position;

        if (NavMesh.SamplePosition(rnd, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void GoToNextPatrolPoint()
    {
        Vector3 targetPos = patrolPoints[patrolIndex].position;

        agent.SetDestination(targetPos);

        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        patrolWaitTimer = patrolWaitTime;
    }

    // ------------------------------------------------------
    // CHASE
    // ------------------------------------------------------

    void ChasePlayer()
    {
        agent.speed = chaseSpeed;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackRange && attackCooldownTimer <= 0)
        {
            EnterAttackState();
            return;
        }

        // ONLY SET DESTINATION IF NEEDED
        if (agent.remainingDistance > stoppingDistance + 0.2f)
            agent.SetDestination(player.position);
    }

    // ------------------------------------------------------
    // ATTACK
    // ------------------------------------------------------

    void EnterAttackState()
    {
        currentState = AIState.Attack;

        lungeTimer = lungeDuration;
        attackCooldownTimer = attackCooldown;

        agent.isStopped = true;
    }

    void HandleAttack()
    {
        lungeTimer -= Time.deltaTime;

        if (lungeTimer > 0)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * attackSpeedMultiplier * Time.deltaTime;
            return;
        }

        agent.isStopped = false;
        StartCoroutine(BackOffRoutine());
        currentState = AIState.Patrol;
    }

    IEnumerator BackOffRoutine()
    {
        currentState = AIState.Scared;

        Vector3 away = (transform.position - player.position).normalized;
        Vector3 target = transform.position + away * 4f;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);

        yield return new WaitForSeconds(3f);

        currentState = AIState.Patrol;
    }

    // ------------------------------------------------------
    // HIDING
    // ------------------------------------------------------

    void TryHideInBush()
    {
        if (hideCooldownTimer > 0) return;

        agent.speed = fleeSpeed;

        if (targetBush == null)
        {
            (BushGroupManager mgr, BushSpot bush) = FindNearestBushSpot();

            if (bush == null)
            {
                Vector3 dir = (transform.position - player.position).normalized;
                Vector3 flee = transform.position + dir * 5f;

                if (NavMesh.SamplePosition(flee, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);

                return;
            }

            targetBush = bush;
            targetBush.isOccupied = true;
        }

        agent.SetDestination(targetBush.GetHidePosition());

        if (Vector3.Distance(transform.position, targetBush.transform.position) < 0.8f)
            EnterHideState();
    }

    void EnterHideState()
    {
        agent.isStopped = true;

        foreach (var r in renderers)
            r.enabled = false;

        currentState = AIState.Hiding;
        hideTimer = hideDuration;
    }

    void HandleHiding()
    {
        hideTimer -= Time.deltaTime;

        if (hideTimer <= 0)
            ExitHideState();
    }

    void ExitHideState()
    {
        foreach (var r in renderers)
            r.enabled = true;

        agent.isStopped = false;

        targetBush.isOccupied = false;
        targetBush = null;

        hideCooldownTimer = hideCooldown;
        currentState = AIState.Patrol;
    }

    // ------------------------------------------------------
    // STATE TRANSITIONS
    // ------------------------------------------------------

    void CheckStateTransitions()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < detectionRange)
        {
            if (playerSpeed > playerSpeedThreshold)
                currentState = AIState.Scared;
            else
                currentState = AIState.Aggro;
        }
    }

    // ------------------------------------------------------
    // HELPERS
    // ------------------------------------------------------

    void HandleEdgeAvoidance()
    {
        Vector3 fwd = transform.forward;

        if (Physics.Raycast(transform.position + Vector3.up * 0.3f, fwd, wallCheckDistance, wallMask))
        {
            agent.ResetPath();
            transform.Rotate(0, Random.Range(-90f, 90f), 0);
        }

        if (!Physics.Raycast(transform.position + fwd * 0.3f + Vector3.up * 0.2f, Vector3.down, edgeCheckDistance, groundMask))
        {
            agent.ResetPath();
            transform.Rotate(0, 180, 0);
        }
    }

    (BushGroupManager, BushSpot) FindNearestBushSpot()
    {
        BushGroupManager[] groups = FindObjectsOfType<BushGroupManager>();
        float best = Mathf.Infinity;
        BushSpot nearest = null;
        BushGroupManager chosen = null;

        foreach (var g in groups)
        {
            BushSpot b = g.GetAvailableBush(transform.position, detectionRange);
            if (b == null) continue;

            float d = Vector3.Distance(transform.position, b.transform.position);
            if (d < best)
            {
                best = d;
                nearest = b;
                chosen = g;
            }
        }

        return (chosen, nearest);
    }
}
