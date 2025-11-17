using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SnakeHandler : MonoBehaviour
{
    // simplified snake spawner used lists instead of queues and removed the whole area spawn system
    // it is a lot better than the previous version
    [Header("Prefab")]
    public GameObject snakePrefab;

    [Header("Spawn Settings")]
    public int initialSpawn = 3;
    public int maxAlive = 10;
    public float spawnInterval = 6f;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Optional: Ground Snap")]
    public bool snapToGround = false;
    public LayerMask groundMask = ~0;
    public float groundCheckDistance = 10f;

    [Header("Optional: NavMesh")]
    public bool requireNavMesh = false;
    public float navMeshSearchDistance = 2f;

    [Header("References")]
    public Transform player;

    private Queue<GameObject> _pool = new Queue<GameObject>();
    private List<GameObject> _activeSnakes = new List<GameObject>();
    private float _spawnTimer = 0f;

    void Start()
    {
        // Validation for snake and spawn points 
        if (snakePrefab == null)
        {
            Debug.LogError("SnakeHandler: No snake prefab assigned!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("SnakeHandler: No spawn points assigned!");
            return;
        }

        // Check for finding player if not assigned 
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // Preload pool of snakes 
        for (int i = 0; i < initialSpawn; i++)
        {
            CreatePooledSnake();
        }

        // Sets Initial spawn of snake
        for (int i = 0; i < initialSpawn; i++)
        {
            SpawnSnake();
        }

        _spawnTimer = spawnInterval;
    }

    void Update()
    {
        // Clean up performance reasons
        _activeSnakes.RemoveAll(s => s == null);

        // Spawn timer
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            _spawnTimer = spawnInterval;
            if (_activeSnakes.Count < maxAlive)
            {
                SpawnSnake();
            }
        }
    }
    // self explanatory it creates a snake to add in the pool
    private GameObject CreatePooledSnake()
    {
        GameObject snake = Instantiate(snakePrefab);

        // Setup collecatable to check if it was collected and then added to the pool 
        var collectable = snake.GetComponent<SnakeCollectable>();
        if (collectable == null)
        {
            collectable = snake.AddComponent<SnakeCollectable>();
        }
        collectable.Initialize(this);

        // Setup ai to so it can work and target player 
        var aiController = snake.GetComponent<AIControllerScript>();
        if (aiController != null && player != null)
        {
            aiController.player = player;
        }

        snake.SetActive(false);
        _pool.Enqueue(snake);
        return snake;
    }

    private GameObject GetSnakeFromPool()
    {
        if (_pool.Count > 0)
        {
            return _pool.Dequeue();
        }
        return CreatePooledSnake();
    }

    public void ReturnToPool(GameObject snake)
    {
        if (snake == null) return;

        snake.SetActive(false);
        _activeSnakes.Remove(snake);
        _pool.Enqueue(snake);
    }

    private bool SpawnSnake()
    {
        if (_activeSnakes.Count >= maxAlive)
        {
            return false;
        }

        // Get random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        if (spawnPoint == null)
        {
            Debug.LogWarning("SnakeHandler: Spawn point is null!");
            return false;
        }

        Vector3 spawnPos = spawnPoint.position;
        Quaternion spawnRot = spawnPoint.rotation;

        // snaps to ground if needed
        if (snapToGround)
        {
            if (Physics.Raycast(spawnPos + Vector3.up * groundCheckDistance, Vector3.down,
                out RaycastHit hit, groundCheckDistance * 2f, groundMask))
            {
                spawnPos = hit.point;
            }
        }

        // this is for adjusting the navmesh postion
        if (requireNavMesh)
        {
            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit navHit, navMeshSearchDistance, NavMesh.AllAreas))
            {
                spawnPos = navHit.position;
            }
            else
            {
                Debug.LogWarning($"SnakeHandler: No NavMesh found near {spawnPoint.name}");
                return false;
            }
        }

        // Gets snakes from pool and activates their position and rotation
        GameObject snake = GetSnakeFromPool();
        snake.transform.SetPositionAndRotation(spawnPos, spawnRot);
        snake.SetActive(true);

        // Reset NavMeshAgent if present
        NavMeshAgent agent = snake.GetComponent<NavMeshAgent>();
        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.Warp(spawnPos);
            }
        }

        _activeSnakes.Add(snake);
        Debug.Log($"SnakeHandler: Spawned snake at {spawnPoint.name}. Active: {_activeSnakes.Count}/{maxAlive}");
        return true;
    }

    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.green;
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.5f);
                Gizmos.DrawLine(point.position, point.position + point.forward * 1f);
            }
        }
    }
}