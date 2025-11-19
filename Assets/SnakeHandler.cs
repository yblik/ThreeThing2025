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

    [Header("Random Offset Settings")]
    public float minOffset = 1f;
    public float maxOffset = 3f;
    public bool avoidOverlap = true;
    public float minSnakeDistance = 2f;

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

    // ===== NEW DIFFICULTY SETTINGS =====
    [Header("Difficulty Scaling")]
    public GameObject[] snakeVariants;          // Assign different snake prefabs here
    public int maxSnakesPerSpawn = 3;           // Max snakes spawned each interval
    public float difficultyScale = 1f;          // External control: 0=easy, 1=hard

    public void IncreasorEffect()
    {
        difficultyScale += 0.2f;
    }
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
        // PROPERLY clean up destroyed snakes (YOUR EXISTING CODE)
        int nullCount = _activeSnakes.RemoveAll(s => s == null);
        if (nullCount > 0)
        {
            Debug.Log($"Cleaned up {nullCount} destroyed snakes. Active: {_activeSnakes.Count}/{maxAlive}");
        }

        // Spawn timer (YOUR EXISTING CODE)
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            _spawnTimer = spawnInterval;

            // ===== MODIFIED SPAWN LOGIC =====
            int snakesToSpawn = GetSnakesToSpawn();
            int spawnedCount = 0;

            for (int i = 0; i < snakesToSpawn; i++)
            {
                if (_activeSnakes.Count < maxAlive)
                {
                    if (SpawnNewSnakeWithVariant())
                        spawnedCount++;
                }
            }

            if (spawnedCount > 0)
            {
                Debug.Log($"Spawned {spawnedCount} snakes (Difficulty: {difficultyScale:F2})");
            }
        }
    }
    // ===== NEW METHOD: Calculate how many snakes to spawn =====
    private int GetSnakesToSpawn()
    {
        // When difficultyScale is 0 -> spawn 1, when 1 -> spawn maxSnakesPerSpawn
        int count = Mathf.RoundToInt(1 + (difficultyScale * (maxSnakesPerSpawn - 1)));
        return Mathf.Clamp(count, 1, maxSnakesPerSpawn);
    }

    // ===== NEW METHOD: Get random snake variant based on difficulty =====
    private GameObject GetSnakeVariant()
    {
        // If no variants or easy difficulty, use original prefab
        if (snakeVariants == null || snakeVariants.Length == 0 || difficultyScale < 0.3f)
            return snakePrefab;

        // Higher difficulty = more likely to spawn variants
        float variantChance = difficultyScale;
        if (Random.value > variantChance)
            return snakePrefab;

        // Pick random variant
        return snakeVariants[Random.Range(0, snakeVariants.Length)];
    }

    // ===== NEW METHOD: Spawn with variant support =====
    private bool SpawnNewSnakeWithVariant()
    {
        GameObject originalPrefab = snakePrefab;
        snakePrefab = GetSnakeVariant(); // Temporary swap

        bool success = SpawnNewSnake(); // Call your EXISTING method

        snakePrefab = originalPrefab; // Restore original
        return success;
    }
    // Add these methods to your existing class:

    private bool SpawnNewSnake()
    {
        if (_activeSnakes.Count >= maxAlive)
        {
            return false;
        }

        // Try multiple spawn points to find a good position
        for (int attempt = 0; attempt < spawnPoints.Length * 2; attempt++)
        {
            // Get random spawn point
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (spawnPoint == null) continue;

            Vector3 spawnPos = spawnPoint.position;
            Quaternion spawnRot = spawnPoint.rotation;

            // Apply random offset
            Vector3 randomOffset = new Vector3(
                Random.Range(-maxOffset, maxOffset),
                0f,
                Random.Range(-maxOffset, maxOffset)
            );

            // Clamp to min/max offset range
            if (randomOffset.magnitude < minOffset)
            {
                randomOffset = randomOffset.normalized * minOffset;
            }
            if (randomOffset.magnitude > maxOffset)
            {
                randomOffset = randomOffset.normalized * maxOffset;
            }

            spawnPos += randomOffset;

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
                    continue; // Try another position if no NavMesh here
                }
            }

            // Check if this position is too close to other snakes
            if (avoidOverlap && IsTooCloseToOtherSnakes(spawnPos))
            {
                continue; // Try another position
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
                Destroy(collectable);
            }

            _activeSnakes.Add(snake);
            Debug.Log($"SnakeHandler: Spawned NEW snake at {spawnPoint.name} with offset. Active: {_activeSnakes.Count}/{maxAlive}");
            return true;
        }

        Debug.LogWarning("SnakeHandler: Could not find valid spawn position after multiple attempts!");
        return false;
    }

    // Check if position is too close to other snakes
    private bool IsTooCloseToOtherSnakes(Vector3 position)
    {
        foreach (GameObject snake in _activeSnakes)
        {
            if (snake != null)
            {
                float distance = Vector3.Distance(position, snake.transform.position);
                if (distance < minSnakeDistance)
                {
                    return true; // Too close to another snake
                }
            }
        }
        return false;
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

                // Draw offset range visualization
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(point.position, minOffset);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(point.position, maxOffset);
            }
        }
    }
}