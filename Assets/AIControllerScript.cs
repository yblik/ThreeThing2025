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
    public float stoppingDistance = 2f;
    public float lungeForce = 4f;
    public float chaseSpeed = 3.5f;
    public float fleeSpeed = 5f;
    public float patrolSpeed = 1.5f;
    public float playerSpeedThreshold = 3.0f;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    private int patrolIndex = 0;
    public float patrolWaitTime = 2f;
    private float patrolWaitTimer;

    [Header("Safety / Unstuck")]
    public float wallCheckDistance = 1f;
    public float edgeCheckDistance = 1.2f;
    public LayerMask groundMask;
    public LayerMask wallMask;

    // Hiding / Bush System
    public float hideDuration = 5f;
    public float hideCooldown = 5f;
    private float hideCooldownTimer;
    private float hideTimer;
    private BushSpot targetBush;


    // attack / lunge
    [Header("Attack Settings")]
    public float attackCooldown = 1.2f;
    private float attackCooldownTimer;
    public float lungeDuration = 0.25f;
    private float lungeTimer;
    public float lungeDistance = 2f;
    public float attackSpeedMultiplier = 6f;


    private Vector3 lastPlayerPos;
    private float playerSpeed;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        renderers = GetComponentsInChildren<MeshRenderer>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        lastPlayerPos = player.position;

        agent.stoppingDistance = stoppingDistance;
    }

    private void Update()
    {
        if (!agent.isOnNavMesh) return;
        if (attackCooldownTimer > 0)
            attackCooldownTimer -= Time.deltaTime;

        // Cooldown handling
        if (hideCooldownTimer > 0)
            hideCooldownTimer -= Time.deltaTime;

        // Player speed
        playerSpeed = (player.position - lastPlayerPos).magnitude / Time.deltaTime;
        lastPlayerPos = player.position;

        HandleEdgeAvoidance();

        if (currentState != AIState.Hiding)
            CheckStateTransitions();

        // State machine
        switch (currentState)
        {
            case AIState.Patrol:
                Patrol();
                break;
            case AIState.Aggro:
                ChasePlayer();
                break;
            case AIState.Scared:
                TryHideInBush();
                break;
            case AIState.Hiding:
                HandleHiding();
                break;
            case AIState.Attack:
                HandleAttack();
                break;
        }
    }

    // -----------------------------------------
    // PATROL SYSTEM
    // -----------------------------------------

    void Patrol()
    {
        agent.speed = patrolSpeed;

        if (patrolPoints.Length == 0)
        {
            RandomWanderPatrol();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0)
                GoToNextPatrolPoint();
        }
    }

    void GoToNextPatrolPoint()
    {
        agent.SetDestination(patrolPoints[patrolIndex].position);
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        patrolWaitTimer = patrolWaitTime;
    }

    void RandomWanderPatrol()
    {
        if (agent.remainingDistance < 1f)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 6f;
            randomDirection += transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
    }

    // -----------------------------------------
    // EDGE & WALL SAFETY
    // -----------------------------------------

    void HandleEdgeAvoidance()
    {
        Vector3 forward = transform.forward;

        // WALL check
        if (Physics.Raycast(transform.position + Vector3.up * 0.3f, forward, wallCheckDistance, wallMask))
        {
            agent.ResetPath();
            transform.Rotate(0, Random.Range(-90f, 90f), 0);
        }

        // EDGE check
        if (!Physics.Raycast(transform.position + forward * 0.3f + Vector3.up * 0.2f, Vector3.down, edgeCheckDistance, groundMask))
        {
            // Don't walk off cliffs
            agent.ResetPath();
            transform.Rotate(0, 180, 0);
        }
    }

    // -----------------------------------------
    // CHASE & ATTACK
    // -----------------------------------------

    void ChasePlayer()
    {
        agent.speed = chaseSpeed;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange && attackCooldownTimer <= 0)
        {
            EnterAttackState();
            return;
        }


        agent.SetDestination(player.position);
    }

    void EnterAttackState()
    {
        currentState = AIState.Attack;
        agent.isStopped = true;

        lungeTimer = lungeDuration;
        attackCooldownTimer = attackCooldown;
    }


    void HandleAttack()
    {
        // Reduce lunge time
        lungeTimer -= Time.deltaTime;

        if (lungeTimer > 0)
        {
            // Perform the lunge
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * attackSpeedMultiplier * Time.deltaTime;
            return;
        }

        // Lunge finished → begin backoff routine
        agent.isStopped = false;
        StartCoroutine(BackOffRoutine());
        currentState = AIState.Patrol;
    }


    private IEnumerator BackOffRoutine()
    {
        currentState = AIState.Scared;

        // move backward a little
        Vector3 away = (transform.position - player.position).normalized;
        Vector3 target = transform.position + away * 4f;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);

        // wait before allowed to attack again
        yield return new WaitForSeconds(5f);

        currentState = AIState.Patrol;  // or AIState.Aggro, your choice
    }


    // -----------------------------------------
    // SCARED / HIDING
    // -----------------------------------------

    void TryHideInBush()
    {
        if (hideCooldownTimer > 0)
            return;

        agent.speed = fleeSpeed;

        if (targetBush == null)
        {
            (BushGroupManager group, BushSpot bush) = FindNearestBushSpot();
            if (bush == null)
            {
                // fallback run-away
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
        foreach (var r in renderers) r.enabled = false;

        currentState = AIState.Hiding;
        hideTimer = hideDuration;
    }

    void HandleHiding()
    {
        hideTimer -= Time.deltaTime;
        if (hideTimer <= 0) ExitHideState();
    }

    void ExitHideState()
    {
        foreach (var r in renderers) r.enabled = true;
        agent.isStopped = false;

        targetBush.isOccupied = false;
        targetBush = null;

        hideCooldownTimer = hideCooldown;

        currentState = AIState.Patrol;
    }

    // -----------------------------------------
    // STATE LOGIC
    // -----------------------------------------

    void CheckStateTransitions()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < detectionRange)
        {
            if (playerSpeed > playerSpeedThreshold)
                currentState = AIState.Scared;
            else
                currentState = AIState.Aggro;
        }
    }

    (BushGroupManager, BushSpot) FindNearestBushSpot()
    {
        BushGroupManager[] groups = FindObjectsOfType<BushGroupManager>();
        BushGroupManager nearestGroup = null;
        BushSpot nearestBush = null;
        float minDist = Mathf.Infinity;

        foreach (var g in groups)
        {
            BushSpot b = g.GetAvailableBush(transform.position, detectionRange);
            if (b == null) continue;

            float d = Vector3.Distance(transform.position, b.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearestBush = b;
                nearestGroup = g;
            }
        }

        return (nearestGroup, nearestBush);
    }
}
