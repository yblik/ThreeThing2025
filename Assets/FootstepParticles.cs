using UnityEngine;
using UnityEngine.AI;

public class FootstepParticles : MonoBehaviour
{
    [Header("References")]
    public Transform footPosition;

    [Header("Timing")]
    public float baseStepInterval = 0.5f;

    private float stepTimer;
    private NavMeshAgent agent;
    private Vector3 lastPos;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        lastPos = transform.position;
    }

    void Update()
    {
        // Determine movement speed — works for both AI and player
        float velocity;
        if (agent != null)
            velocity = agent.velocity.magnitude;
        else
            velocity = (transform.position - lastPos).magnitude / Time.deltaTime;

        lastPos = transform.position;

        if (velocity < 0.2f)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;

        // Use agent.speed if available; otherwise assume 5 m/s max for player
        float maxSpeed = agent ? agent.speed : 5f;
        float interval = Mathf.Lerp(baseStepInterval * 1.5f, baseStepInterval * 0.5f, velocity / maxSpeed);

        if (stepTimer <= 0f)
        {
            TrySpawnStepEffect();
            stepTimer = interval;
        }
    }

    void TrySpawnStepEffect()
    {
        RaycastHit hit;
        Vector3 origin = footPosition ? footPosition.position : transform.position + Vector3.up * 0.2f;

        if (Physics.Raycast(origin, Vector3.down, out hit, 2f))
        {
            if (hit.collider.CompareTag("Sand") && FootstepParticlePool.Instance != null)
                FootstepParticlePool.Instance.PlayAt(hit.point);
        }
    }
}
