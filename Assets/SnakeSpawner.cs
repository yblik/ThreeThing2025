using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI;
using System.Runtime.CompilerServices;
using UnityEngine.AI;

public class SnakeSpawner : MonoBehaviour
{
    //class that spawns snakes and manages their positioning i.e., going underground or not it
    // Unsure if this is fully correct or not and idk if we are using spawn points or just area spawning
    [Header("Prefab")]
    public GameObject snakePrefab;

    [Header("Spawn Settings")]
    public int initialSpawn = 3;
    public int maxAlive = 10;
    public float spawnInterval = 6f;

    //Using points for spawning snakes
    [Header("Spawn Location")]
    public bool useSpawnPoints = false;
    public Transform[] spawnPoints;

    // Incase we are not using spawn points, define area
    public Vector3 areaCenter;
    public Vector3 areaSize = new Vector3(12, 0, 12);

    [Header("Ground Settings")]
    public bool projectToGround = true;
    public LayerMask groundMask = ~0;
    public float groundRayCastHeight = 3f;
    public bool alignToSurface = false;

    [Header("NavMesh")]
    public bool requireNavMesh = true;
    public float navMeshMaxDistance = 2f;

    [Header("Separation")]
    public float minSeparationDistance = 0.5f;
    public float separationProbeRadius = 0.3f;
    public LayerMask separationMask = ~0;

    // Underground calculations for snakes might be better if an animation played instead of doing transforms?
    [Header("Underground Settings")]
    public bool UndergroundEnable = true;
    [Range(0f, 1f)] public float spawnUnderground = 0.25f;
    public float undergroundYOffset = -1.5f; //snake size
    public Vector2 unburrowDelayRange = new Vector2(3f, 8f);
    public bool reburrowAfterUnburrow = false;
    public Vector2 reburrowDelayRange = new Vector2(6f, 12f);

    // this basically just handles the ammount of snakes loaded in memory
    [Header("Pooling")]
    public int preload = 0;
    private readonly Queue<GameObject> _pool = new Queue<GameObject>();
    private readonly Queue<GameObject> _alive = new Queue<GameObject>();
    private float _spawnTimer = 0f;

    // This validates the snake prefab then spawns a pool of said snakes it's done through a spawn timer 
    void Start()
    {
        if (snakePrefab == null)
        {
            Debug.LogError("SnakeSpawner: No snake prefab assigned.");
            return;
        }

        PreLoadPool(Mathf.Max(preload, initialSpawn));

        for (int i = 0; i < initialSpawn; i++)
        {
            SpawnSnake();
        }
        _spawnTimer = spawnInterval;



    }
    // this just removes the remaining snakes from queue memory and then attempts to spawn snake on set intervals
    void Update()
    {
        //cleans up remaining snakes from queue memory 
        while (_alive.Count > 0 && (_alive.Peek() == null || !_alive.Peek().activeInHierarchy))
        {
            _alive.Dequeue();
        }

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            _spawnTimer = spawnInterval;
            SpawnSnake();

        }
    }

    private void PreLoadPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(snakePrefab);
            go.SetActive(false);
            _pool.Enqueue(go);
        }
    }

    private GameObject GetFromPool()
    {
        if (_pool.Count > 0)
        {
            return _pool.Dequeue();
        }
        else
        {
            var go = Instantiate(snakePrefab);
            go.SetActive(false);
            return go;
        }
    }

    private void ReturnToPool(GameObject go)
    {
        if (!go) return;
        go.SetActive(false);
        _pool.Enqueue(go);

    }
    // this calls the spawn position function and handles nav mesh agents and underground spawning 
    public bool SpawnSnake()
    {
        if (_alive.Count >= maxAlive) return false;
        if (!FindSpawnPosition(out Vector3 pos, out Quaternion rot))
            return false;
        var go = GetFromPool();
        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);

        _alive.Enqueue(go);

        //resets NavMeshAgent if available 
        var agent = go.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
         if (agent.enabled && agent.isOnNavMesh)
                agent.Warp(pos);
        }

        //Underground handling
        bool undergroundAtSpawn = UndergroundEnable && Random.value < spawnUnderground;
        var burrow = go.GetComponent<SnakeBurrow>();
        if (burrow == null) burrow = go.AddComponent<SnakeBurrow>();
        burrow.Configure(this, pos); // remember "groundY" at spawn

        if (undergroundAtSpawn) burrow.BuryImmediate();
        else burrow.AppearImmediate();

        return true;
    }

    private bool FindSpawnPosition(out Vector3 position, out Quaternion rotation)
    {
        // Base position
        Vector3 basePos;
        if (useSpawnPoints && spawnPoints != null && spawnPoints.Length > 0)
        {
            var t = spawnPoints[Random.Range(0, spawnPoints.Length)];
            basePos = t ? t.position : transform.position;
        }
        else
        {
            Vector3 half = areaSize * 0.5f;
            Vector3 rndLocal = new Vector3(
                Random.Range(-half.x, half.x),
                0,
                Random.Range(-half.z, half.z)
            );
            basePos = transform.TransformPoint(areaCenter + rndLocal);
        }

        Vector3 pos = basePos;
        Vector3 up = Vector3.up;

        // Projects to ground
        if (projectToGround)
        {
            Vector3 rayOrgin = basePos + Vector3.up * groundRayCastHeight;
            if (Physics.Raycast(rayOrgin, Vector3.down, out RaycastHit hit, groundRayCastHeight * 2f, groundMask))
            {
                pos = hit.point;
                if (alignToSurface)
                {
                    up = hit.normal;
                }
            }
            else
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
                return false;
            }
        }

        //separation checking 
        if (minSeparationDistance > 0f)
        {
            Collider[] colliders = Physics.OverlapSphere(pos, separationProbeRadius, separationMask);
            foreach (var col in colliders)
            {
                if (Vector3.Distance(col.ClosestPoint(pos), pos) < minSeparationDistance)
                {
                    position = Vector3.zero;
                    rotation = Quaternion.identity;
                    return false;
                }
            }
        }
    //NavMesh stuff
    if (requireNavMesh)
        {
            if (NavMesh.SamplePosition(pos, out NavMeshHit navHit, navMeshMaxDistance, NavMesh.AllAreas))
            {
                pos = navHit.position;
            }
            else
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
                return false;
            }
        }
        // assigning parameters for FindSpawnPosition
        position = pos;
    rotation = alignToSurface
        ? Quaternion.FromToRotation(Vector3.up, up) * Quaternion.Euler(0, Random.Range(0, 360f), 0)
        : Quaternion.Euler(0, Random.Range(0, 360f), 0);
    return true;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        if (useSpawnPoints && spawnPoints != null && spawnPoints.Length > 0)
        {
            foreach (var t in spawnPoints)
            {
                if (!t) continue;
                Gizmos.DrawWireSphere(t.position, 0.3f);
            }
        }
        else
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(areaCenter + new Vector3(0, 0.01f, 0), new Vector3(areaSize.x, 0.02f, areaSize.z));
        }
    }
}



//helper class 
public class SnakeBurrow : MonoBehaviour
{
    private SnakeSpawner _spawner;
    private Renderer[] _renderers;
    private Collider[] _colliders;
    private NavMeshAgent _agent;
    private float _groundY;
    private Coroutine _routine;

    public void Configure(SnakeSpawner spawner, Vector3 spawnPos)
    {
        _spawner = spawner;
        _groundY = spawnPos.y;
        _renderers = GetComponentsInChildren<Renderer>(true);
        _colliders = GetComponentsInChildren<Collider>(true);
        _agent = GetComponent<NavMeshAgent>();
    }

    public void BuryImmediate()
    {
        if (_spawner == null) return;

        // Move down to simulate underground
        Vector3 p = transform.position;
        p.y = _groundY + _spawner.undergroundYOffset;
        transform.position = p;

        SetVisible(false);
        SetCollidable(false);

        if (_routine != null) StopCoroutine(_routine);
        float delay = Random.Range(_spawner.unburrowDelayRange.x, _spawner.unburrowDelayRange.y);
        _routine = StartCoroutine(UnburrowAfter(delay));
    }

    public void AppearImmediate()
    {
        if (_spawner == null) return;

        Vector3 p = transform.position;
        p.y = _groundY; // back to surface
        transform.position = p;

        SetVisible(true);
        SetCollidable(true);

        if (_spawner.reburrowAfterUnburrow)
        {
            if (_routine != null) StopCoroutine(_routine);
            float delay = Random.Range(_spawner.reburrowDelayRange.x, _spawner.reburrowDelayRange.y);
            _routine = StartCoroutine(BurrowAfter(delay));
        }
    }

    private IEnumerator UnburrowAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        AppearImmediate();
    }

    private IEnumerator BurrowAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        BuryImmediate();
    }

    private void SetVisible(bool visible)
    {
        if (_renderers == null) return;
        foreach (var r in _renderers) if (r) r.enabled = visible;
    }

    private void SetCollidable(bool collidable)
    {
        if (_colliders != null)
            foreach (var c in _colliders) if (c) c.enabled = collidable;

        if (_agent != null)
        {
            // Keep agent on the NavMesh, just stop while burrowed
            _agent.isStopped = !collidable;
        }
    }
}
