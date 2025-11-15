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
    public void Switch()
    {
        if (roomNumber == 0)
        {
            SceneManager.LoadScene("Tent");
        }
        if (roomNumber == 1)
        {
            SceneManager.LoadScene("Scene01");
        }
        if (roomNumber == 2) //must be set - this is a bad system I should be shot
        {
            SceneManager.LoadScene("Hospital");
        }
    }
    //instead of void start
    public void BootlegStart()
    {
        UnlockMovement();
        LeftRoomSpawnPos();


    }
    private void UnlockMovement()
    {
        movementScript.EnableMovement();
    }
    private void LeftRoomSpawnPos()
    {
        roomRefresh.SetStartPos(PlayerPrefs.GetInt("SpawnPoint")); //to be disabled after

        //wake up code here
        //
        PlayerPrefs.SetInt("SpawnPoint", 0); //tlike here

    }
    public void Muertes() //called by health script on death
    {
        PlayerPrefs.SetInt("SpawnPoint", 1); //set to hospital bed
        roomNumber = 2;

    }
    
}
