using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchRoom : MonoBehaviour
{
    public int roomNumber;
    public ThirdPersonMovement movementScript;

    public RoomRefresh roomRefresh;
    public Health hp;
    public void Switch() //has to be this since animation event
    {
            SpawnManager.Instance.SetSpawnPoint(roomNumber); // or whatever logic

        if (roomNumber == 0)
        {
            SceneManager.LoadScene("Tent");
        }
        if (roomNumber == 1)
        {
            SceneManager.LoadScene("Scene01 1");
        }
        if (roomNumber == 2) //must be set - this is a bad system I should be shot
        {
            SceneManager.LoadScene("Hospital");
        }
        if (roomNumber == 3) 
        {
            SceneManager.LoadScene("Shop");
        }
        if (roomNumber == 4)
        {
            SceneManager.LoadScene("Casino");
        }
    }
    //instead of void start
    public void BootlegStart()
    {
        roomRefresh.SetStartPos();
        if (SpawnManager.Instance.GetRespawnPoint() == false)
        {
            UnlockMovement();
        }
        if (SpawnManager.Instance.GetRespawnPoint() == true)
        { 
            hp.sleep(0); //wait till fully awaken   
        }


    }
    private void UnlockMovement()
    {
        movementScript.EnableMovement();
    }
    public void Muertes()
    {
        SpawnManager.Instance.SetSpawnPoint(2); // Hospital
        SpawnManager.Instance.SetPoint(1); // Hospital bed
        //SceneManager.LoadScene("Hospital");
    }

}
