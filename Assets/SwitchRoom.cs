using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchRoom : MonoBehaviour
{
    public int roomNumber;
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
    }
}
