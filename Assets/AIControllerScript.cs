using UnityEngine;
using UnityEngine.AI;

public class AIControllerScript : MonoBehaviour
{
    public enum AIState { Aggro, Scared }

    [Header("References")]
    public Transform player;
    private NavMeshAgent agent;

    [Header("AI Settings")]
    public AIState currentState = AIState.Aggro;
    public float detectionRange = 15f;
    public float fleeDistance = 10f;
    public float chaseSpeed = 3.5f;
    public float fleeSpeed = 5f;
    public float playerSpeedThreshold = 3.0f;

    private Vector3 lastPlayerPos;
    private float playerSpeed;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        lastPlayerPos = player.position;

        // Ensure agent starts on NavMesh
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
        // Ensure we're still on the NavMesh
        if (!agent.isOnNavMesh)
        {
            Debug.LogError($"{name} is not on NavMesh!");
            return;
        }

        // Calculate player movement speed
        playerSpeed = ((player.position - lastPlayerPos).magnitude) / Time.deltaTime;
        lastPlayerPos = player.position;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > detectionRange)
        {
            agent.ResetPath();
            return;
        }

        // Switch between states based on player speed (optional logic)
        if (currentState == AIState.Aggro && playerSpeed > playerSpeedThreshold)
        {
            currentState = AIState.Scared;
        }
        else if (currentState == AIState.Scared && playerSpeed < playerSpeedThreshold / 2f)
        {
            currentState = AIState.Aggro;
        }

        // Perform the current state’s behavior
        switch (currentState)
        {
            case AIState.Aggro:
                ChasePlayer();
                break;
            case AIState.Scared:
                FleeFromPlayer();
                break;
        }
    }

    void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }

    void FleeFromPlayer()
    {
        agent.speed = fleeSpeed;

        Vector3 directionAway = (transform.position - player.position).normalized;
        Vector3 fleeTarget = transform.position + directionAway * fleeDistance;

        if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
