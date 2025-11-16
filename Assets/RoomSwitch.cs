using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSwitch : MonoBehaviour
{
    public Animator Transition;

    [Header("Save")]
    public Health health;
    public PlayerCatch catcher;
    public Bank natwest;

    public int roomNumber; // 0 = tent, 1 = outside, 2 = hospital (always the target)
    public SwitchRoom SwitchRom;
    public bool returnFromTent;
    public bool returnFromHospital;
    public bool returnFromShop;
    public bool returnFromCasino;


    public void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            SpawnManager.Instance.SetSpawnPoint(0);
            // Assuming you have a method to switch rooms, call it here
            SwitchRom.roomNumber = roomNumber; 
            if (returnFromTent == true)
            {
                SpawnManager.Instance.SetPoint(0); // Tent
            }
            if (returnFromHospital == true)
            {
                SpawnManager.Instance.SetPoint(1); // Hospital

            }
            if (returnFromShop == true)
            {
                SpawnManager.Instance.SetPoint(2); // Shop
            }
            if (returnFromCasino == true)
            {
                SpawnManager.Instance.SetPoint(3); // Casino
            }
            Transition.Play("SwitchRoom");
            SaveData();
        }
    }
    public void SaveData()
    {
        health.SaveHealth();
        catcher.SaveSnakeAmount();
        natwest.SaveMoney();
    }
}
