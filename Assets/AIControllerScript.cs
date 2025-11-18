using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AIControllerScript : MonoBehaviour
{
    public enum AIState { Patrol, Aggro, Scared, Hiding, Attack }

    [Header("References")]
    public Transform player;
    private NavMeshAgent agent;
    public SnakeAttack SA;

    [Header("AI Settings")]
    public AIState currentState = AIState.Patrol;
    public float detectionRange = 15f;
    public float attackRange = 3f;
    public float stoppingDistance = 1.2f;
    public float chaseSpeed = 3.5f;
    public float patrolSpeed = 1.5f;

    public Animator snake;
    public Collider snakeCollider;

    [Header("Behavior")]
    public bool aggressive = true;
    public bool wanderer = true;

    [Header("Auto Disappear")]
    public float timeBeforeDisappear = 30f;
    private float disappearTimer;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    private int patrolIndex = 0;
    private float patrolWaitTimer;
    public float patrolWaitTime = 2f;

    [Header("Attack Settings")]
    public float attackCooldown = 1.2f;
    private float attackCooldownTimer;
    public float lungeDuration = 0.25f;
    private float lungeTimer;
    public float attackSpeedMultiplier = 6f;

    [Header("Rotation Smoothing")]
    public float turnSpeed = 8f;
    public float minMoveToRotate = 0.05f;

    private Quaternion targetRotation;
    private bool isLunging = false;
    public bool isBurrowed = false;

    // ADD BACK STATE CHECK TIMER
    private float stateCheckTimer = 0.5f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        agent.stoppingDistance = stoppingDistance;
        agent.updateRotation = false;
        agent.autoBraking = false;

        targetRotation = transform.rotation;

        // Start disappear timer
        disappearTimer = timeBeforeDisappear;
    }

    public void ResetOnSpawn()
    {
        StopAllCoroutines();

        currentState = AIState.Patrol;

        attackCooldownTimer = 0f;
        lungeTimer = 0f;
        isLunging = false;
        isBurrowed = false;

        // RESET DISAPPEAR TIMER
        disappearTimer = timeBeforeDisappear;

        // RESET STATE CHECK TIMER
        stateCheckTimer = 0.5f;

        if (agent != null)
        {
            agent.enabled = true;
            agent.ResetPath();
            agent.isStopped = false;
            agent.updatePosition = true;
            agent.updateRotation = false;
            agent.speed = patrolSpeed;
            agent.stoppingDistance = stoppingDistance;

            // IMMEDIATELY SET A PATROL DESTINATION ON SPAWN
            SetRandomDestination();
        }

        if (snake != null)
        {
            snake.enabled = true;
            snake.Rebind();
        }

        if (snakeCollider != null)
            snakeCollider.enabled = true;
        BurrowOut();
    }

    void SetRandomDestination()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, patrolPoints.Length);
            agent.SetDestination(patrolPoints[randomIndex].position);
        }
        else
        {
            // Random wander point
            Vector3 rnd = Random.insideUnitSphere * 3f + transform.position;
            if (NavMesh.SamplePosition(rnd, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
    }

    private void Update()
    {
        if (!agent.isOnNavMesh) return;

        attackCooldownTimer = Mathf.Max(0f, attackCooldownTimer - Time.deltaTime);

        // DISAPPEAR TIMER - Count down and hide when time's up
        disappearTimer -= Time.deltaTime;
        if (disappearTimer <= 0f && !isBurrowed)
        {
            BurrowAndDisappear();
            return;
        }

        // STATE CHECK WITH TIMER (FIXES FREEZING)
        stateCheckTimer -= Time.deltaTime;
        if (stateCheckTimer <= 0f)
        {
            CheckStateTransitions();
            stateCheckTimer = 0.5f; // Check every 0.5 seconds
        }

        switch (currentState)
        {
            case AIState.Patrol: Patrol(); break;
            case AIState.Aggro: ChasePlayer(); break;
            case AIState.Attack: HandleAttack(); break;
        }

        SmoothRotateWithMovement();
    }

    void CheckStateTransitions()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < detectionRange && aggressive && currentState != AIState.Attack)
        {
            currentState = AIState.Aggro;
        }
        else if (currentState != AIState.Attack) // Don't interrupt attack
        {
            currentState = AIState.Patrol;
        }
    }

    void Patrol()
    {
        if (!wanderer) return;

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
        patrolWaitTimer = patrolWaitTime;
    }

    void RandomWanderPatrol()
    {
        if (agent.remainingDistance > 0.5f && agent.hasPath) return;

        Vector3 rnd = Random.insideUnitSphere * 6f + transform.position;
        if (NavMesh.SamplePosition(rnd, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

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

        agent.SetDestination(player.position);
    }

    void EnterAttackState()
    {
        currentState = AIState.Attack;
        lungeTimer = lungeDuration;
        attackCooldownTimer = attackCooldown;

        isLunging = true;
        agent.isStopped = true;
        agent.updatePosition = false;
    }

    void HandleAttack()
    {
        lungeTimer -= Time.deltaTime;

        if (lungeTimer > 0f)
        {
            // Lunge toward player
            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0;
            transform.position += dir * attackSpeedMultiplier * Time.deltaTime;
            return;
        }

        // ATTACK FINISHED - GO BACK TO CHASING
        isLunging = false;
        agent.updatePosition = true;
        agent.isStopped = false;

        // IMMEDIATELY GO BACK TO AGGRO STATE, NOT PATROL
        currentState = AIState.Aggro;
    }

    // REMOVE THE BackOffRoutine COROUTINE COMPLETELY - IT'S CAUSING ISSUES

    IEnumerator BackOffRoutine()
    {
        yield return new WaitForSeconds(1f);
        currentState = AIState.Patrol;
    }

    void BurrowAndDisappear()
    {
        if (snakeCollider != null)
            snakeCollider.enabled = false;

        agent.isStopped = true;
        BurrowIn();

        // Destroy after animation
        StartCoroutine(DestroyAfterAnimation());
    }

    IEnumerator DestroyAfterAnimation()
    {
        // Wait for burrow animation to complete
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

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

    void BurrowIn()
    {
        if (!snake.enabled) snake.enabled = true;
        snake.Play("BurrowIn");
        isBurrowed = true;
    }

    void BurrowOut()
    {
        if (!snake.enabled) snake.enabled = true;
        snake.Play("BurrowOut");
        isBurrowed = false;
    }

    public void FreezeForCollection()
    {
        // Stop all coroutines first
        StopAllCoroutines();

        // Completely freeze NavMeshAgent
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        // Freeze current state - prevent any state transitions
        currentState = AIState.Patrol; // Set to a neutral state

        // Disable AI script components but keep this script enabled for collection logic
        enabled = false; // This disables the Update() method

        // Stop any attack or movement
        isLunging = false;
        snake.Play("trapped");

        // Optional: Play a "frozen" or "stunned" animation instead of burrow
        if (snake != null)
        {
            snake.Play("Idle"); // Or create a "Stunned" animation
        }

        // Make sure collider is enabled for collection
        if (snakeCollider != null)
            snakeCollider.enabled = true;

        Debug.Log("Snake frozen for collection");
    }
}