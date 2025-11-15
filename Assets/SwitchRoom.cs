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
            PlayerPrefs.GetInt("SpawnPoint", 0); //same but different
        }
        if (roomNumber == 1)
        {
            SceneManager.LoadScene("Scene01");
            PlayerPrefs.GetInt("SpawnPoint", 0); //same but different
        }
        if (roomNumber == 2)
        {
            SceneManager.LoadScene("Hospital");
            PlayerPrefs.SetInt("SpawnPoint", 1);
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
        roomRefresh.SetStartPos(PlayerPrefs.GetInt("SpawnPoint"));
    }
}
