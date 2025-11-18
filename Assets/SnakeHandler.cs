using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SnakeHandler : MonoBehaviour
{
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

    private List<GameObject> _activeSnakes = new List<GameObject>();
    private float _spawnTimer = 0f;

    void Start()
    {
        // Validation
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

        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // Initial spawn
        for (int i = 0; i < initialSpawn; i++)
        {
            SpawnNewSnake();
        }

        _spawnTimer = spawnInterval;
    }

    void Update()
    {
        // Clean up destroyed snakes
        _activeSnakes.RemoveAll(s => s == null);

        // Spawn timer
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            _spawnTimer = spawnInterval;
            if (_activeSnakes.Count < maxAlive)
            {
                SpawnNewSnake();
            }
        }
    }

    private bool SpawnNewSnake()
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

        // Snap to ground if needed
        if (snapToGround)
        {
            if (Physics.Raycast(spawnPos + Vector3.up * groundCheckDistance, Vector3.down,
                out RaycastHit hit, groundCheckDistance * 2f, groundMask))
            {
                spawnPos = hit.point;
            }
        }

        // Adjust for NavMesh
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

        // INSTANTIATE NEW SNAKE (no pooling)
        GameObject snake = Instantiate(snakePrefab, spawnPos, spawnRot);

        // Setup AI controller
        var aiController = snake.GetComponent<AIControllerScript>();
        if (aiController != null && player != null)
        {
            aiController.player = player;
            aiController.SA.player = player;
            aiController.ResetOnSpawn();
        }

        // Remove or modify the SnakeCollectable component since we're not pooling
        var collectable = snake.GetComponent<SnakeCollectable>();
        if (collectable != null)
        {
            // Either destroy it or modify it to destroy the snake instead of returning to pool
            Destroy(collectable);

            // Alternative: Replace with simple destroy-on-collect behavior
            // var newCollectable = snake.AddComponent<SimpleDestroyCollectable>();
            // newCollectable.Initialize(this);
        }

        _activeSnakes.Add(snake);
        Debug.Log($"SnakeHandler: Spawned NEW snake at {spawnPoint.name}. Active: {_activeSnakes.Count}/{maxAlive}");
        return true;
    }

    // Optional: Method to manually remove/destroy a snake
    public void RemoveSnake(GameObject snake)
    {
        if (snake != null)
        {
            _activeSnakes.Remove(snake);
            Destroy(snake);
        }
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