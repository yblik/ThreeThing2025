using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchRoom : MonoBehaviour
{
    public int roomNumber;

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
    public void LeftRoomSpawnPos()
    {
        roomRefresh.SetStartPos(PlayerPrefs.GetInt("SpawnPoint"));
    }
}
