using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomRefresh : MonoBehaviour
{
    public Transform player; //target

    public Transform Tent; // either entering or leaving tent
    public Transform Spawn; // bed in tent / respawn point in hospital scene


    public Transform Hospital; // respawn point / either entering or leaving tent
    public Transform Casino; // either entering or leaving tent
    public int wah;


    public void SetStartPos() //for leaving back into scene in tent scenes these may never be used
    {
        int Index = SpawnManager.Instance.GetPoint();
        if (Index == 0)
        {
            player.position = Tent.position;
        }
        if (Index == 1)
        {
            player.position = Spawn.position;
        }
        if (Index == 2)
        {
            player.position = Hospital.position;
        }
        if (Index == 3)
        {
            player.position = Casino.position;
        }
    }
}
