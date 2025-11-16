using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public int spawnIndex = 0; // rooms
    public int posIndex = 0; // positions

    public bool spawn; // 0 = normal, 1 = spawn

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SetPoint(1); //set to bed on game session
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
    public void SetPoint(int index)
    {
        posIndex = index;
    }

    public int GetPoint()
    {
        return posIndex;
    }

    public void SetRespawn(bool set)
    {
        spawn = set;
    }

    public bool GetRespawnPoint()
    {
        return spawn;
    }
}
