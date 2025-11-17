using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    public int spawnIndex = 0;
    public int posIndex = 0;
    public bool spawn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return; // <-- IMPORTANT
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetPoint(1);
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
