using UnityEngine;
using UnityEngine.AI;

public class AIControllerScript : MonoBehaviour
{
    public enum AIState { Aggro, Scared, Hiding }

    [Header("References")]
    public Transform player;
    private NavMeshAgent agent;
    private MeshRenderer[] renderers;

    [Header("AI Settings")]
    public AIState currentState = AIState.Aggro;
    public float detectionRange = 15f;
    public float fleeDistance = 10f;
    public float chaseSpeed = 3.5f;
    public float fleeSpeed = 5f;
    public float playerSpeedThreshold = 3.0f;
    public float hideDuration = 5f;
    public float hideCooldownTime = 5f;

    private Vector3 lastPlayerPos;
    private float playerSpeed;

    // Bush system
    private BushSpot targetBush;
    private BushGroupManager targetGroup;
    private float hideTimer;
    private float hideCooldownTimer = 0f; // tracks cooldown after emerging

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        renderers = GetComponentsInChildren<MeshRenderer>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        lastPlayerPos = player.position;

        // Ensure the agent starts on a valid NavMesh
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        else
        {
            Debug.LogError($"{name} could NOT find NavMesh at start!");
        }
    }

    void Update()
    {
        if (!agent.isOnNavMesh)
        {
            Debug.LogError($"{name} is not on NavMesh!");
            return;
        }

        // Reduce hide cooldown timer every frame
        if (hideCooldownTimer > 0f)
            hideCooldownTimer -= Time.deltaTime;

        // Calculate player movement speed
        playerSpeed = ((player.position - lastPlayerPos).magnitude) / Time.deltaTime;
        lastPlayerPos = player.position;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Out of detection range: idle / no path
        if (distanceToPlayer > detectionRange && currentState != AIState.Hiding)
        {
            agent.ResetPath();
            return;
        }

        // State transitions
        if (currentState == AIState.Aggro && playerSpeed > playerSpeedThreshold)
        {
            currentState = AIState.Scared;
        }
        else if (currentState == AIState.Scared && playerSpeed < playerSpeedThreshold / 2f)
        {
            currentState = AIState.Aggro;
        }

        // Run behavior
        switch (currentState)
        {
            case AIState.Aggro:
                ChasePlayer();
                break;

            case AIState.Scared:
                TryHideInBush();
                break;

            case AIState.Hiding:
                HandleHiding();
                break;
        }
    }

    void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        if (agent.isOnNavMesh)
            agent.SetDestination(player.position);
    }

    void TryHideInBush()
    {
        // Respect cooldown — can't hide again until timer runs out
        if (hideCooldownTimer > 0f)
            return;

        agent.speed = fleeSpeed;

        // If we already have a target bush, move to it
        if (targetBush != null)
        {
            if (agent.isOnNavMesh)
                agent.SetDestination(targetBush.GetHidePosition());

            float dist = Vector3.Distance(transform.position, targetBush.transform.position);
            if (dist < 0.8f)
                EnterHideState();

            return;
        }

        // Otherwise, find nearest available bush
        (BushGroupManager group, BushSpot bush) = FindNearestBushSpot();

        if (bush != null)
        {
            targetBush = bush;
            targetGroup = group;
            targetBush.isOccupied = true;

            if (agent.isOnNavMesh)
                agent.SetDestination(targetBush.GetHidePosition());
        }
        else
        {
            // No bushes found — fallback to running away
            Vector3 directionAway = (transform.position - player.position).normalized;
            Vector3 fleeTarget = transform.position + directionAway * fleeDistance;

            if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
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

        if (hideTimer <= 0f)
        {
            ExitHideState();
        }
    }

    void ExitHideState()
    {
        agent.isStopped = false;

        foreach (var r in renderers)
            r.enabled = true;

        // Release bush
        if (targetBush)
        {
            targetBush.isOccupied = false;
            targetBush = null;
        }
        targetGroup = null;

        //  Start cooldown timer
        hideCooldownTimer = hideCooldownTime;

        currentState = AIState.Aggro;
    }

    (BushGroupManager, BushSpot) FindNearestBushSpot()
    {
        BushGroupManager[] groups = FindObjectsOfType<BushGroupManager>();
        BushGroupManager nearestGroup = null;
        BushSpot nearestBush = null;
        float minDist = Mathf.Infinity;

        foreach (var group in groups)
        {
            BushSpot candidate = group.GetAvailableBush(transform.position, detectionRange);
            if (candidate == null) continue;

            float dist = Vector3.Distance(transform.position, candidate.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestGroup = group;
                nearestBush = candidate;
            }
        }

        return (nearestGroup, nearestBush);
    }
}
