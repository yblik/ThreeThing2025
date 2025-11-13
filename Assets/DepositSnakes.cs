using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepositSnakes : MonoBehaviour
{
    public Transform player;   
    public float triggerDistance = 3; 

    public int depositedSnakes = 0;
    public PlayerCatch PC;
    public Bank Natwest;


    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= triggerDistance && depositedSnakes < 20)
        {
            print("can deposit rn");
            if (Input.GetKeyDown(KeyCode.E) && PC.StroredCollectibles > 0)
            {
                if (PC.StroredCollectibles > 0)
                {
                    Debug.Log("Deposited a collectible!");
                    depositedSnakes += PC.StroredCollectibles;
                    Natwest.WorkdaysWorking(PC.StroredCollectibles); //each snake is worth 10 dinero
                    PC.StroredCollectibles = 0;
                }
            }
        }
    }
    //then code to display depositied snakes on screen


}
