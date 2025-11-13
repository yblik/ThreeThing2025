using System.Collections.Generic;
using UnityEngine;

public class FootstepParticlePool : MonoBehaviour
{
    public static FootstepParticlePool Instance;

    [Header("Pool Settings")]
    public ParticleSystem prefab;
    public int poolSize = 15;

    private readonly Queue<ParticleSystem> pool = new();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Pre-instantiate the pool
        for (int i = 0; i < poolSize; i++)
        {
            var ps = Instantiate(prefab, transform);
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.gameObject.SetActive(false);
            pool.Enqueue(ps);
        }
    }

    public void PlayAt(Vector3 position)
    {
        if (pool.Count == 0)
        {
            Debug.LogWarning("Particle pool exhausted — consider increasing pool size.");
            return;
        }

        var ps = pool.Dequeue();
        ps.transform.position = position;
        ps.gameObject.SetActive(true);
        ps.Play();

        StartCoroutine(ReturnToPool(ps, ps.main.duration + ps.main.startLifetime.constantMax));
    }

    private System.Collections.IEnumerator ReturnToPool(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
        pool.Enqueue(ps);
    }
}
