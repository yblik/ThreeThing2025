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
    public bool returnFromHospital;


    public void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            // Assuming you have a method to switch rooms, call it here
            SwitchRom.roomNumber = roomNumber; 
            if (returnFromHospital == true)
            {
                SpawnManager.Instance.SetSpawnPoint(2); // Hospital

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
