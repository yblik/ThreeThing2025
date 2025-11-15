using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomRefresh : MonoBehaviour
{
    public Transform player;
    public Transform outsideTewnt;
    public bool worksonce = false;


    private void Update()
    {
        if (!worksonce)
        {
            player.position = outsideTewnt.position;
            print("work");
            worksonce = true;
        }
    }
}
