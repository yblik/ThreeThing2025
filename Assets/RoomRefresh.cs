using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomRefresh : MonoBehaviour
{
    public Transform player;
    public Transform outsideTewnt;

    public void Start()
    {
        player.position = outsideTewnt.position;
    }
}
