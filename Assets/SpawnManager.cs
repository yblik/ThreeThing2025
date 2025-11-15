using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public int spawnIndex = 0; // 0 = normal, 1 = spawn

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // persists across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSpawnPoint(int index)
    {
        spawnIndex = index;
    }

    public int GetSpawnPoint()
    {
        return spawnIndex;
    }
}
