using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int storage;
    public int increasers;
    public int traps;

    public bool MainGame; //so no one places stuff in tents

    private void Awake()
    {
        storage = PlayerPrefs.GetInt("item0");
        increasers = PlayerPrefs.GetInt("item1");
        traps = PlayerPrefs.GetInt("item2");
    }

    public void SaveInventory() 
    {
        print("should save");
        PlayerPrefs.SetInt("item0", storage);
        PlayerPrefs.SetInt("item1", increasers);
        PlayerPrefs.SetInt("item2", traps);

    }
}
