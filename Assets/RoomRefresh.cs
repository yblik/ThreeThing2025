using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomRefresh : MonoBehaviour
{
    public Transform player;
    public Transform outsideTewnt;

    public void Awake()
    {
        player.position = outsideTewnt.position;
        print("work");
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            player.position = outsideTewnt.position;
            print("work");
        }
    }
}
