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


    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Assuming you have a method to switch rooms, call it here
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
