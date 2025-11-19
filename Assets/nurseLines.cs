using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nurseLines : MonoBehaviour
{
    public Animator speak;

    public GameObject patchedUp;


    public GameObject talking;

    public void Awake()
    {
        if (SpawnManager.Instance.GetRespawnPoint() == true)
        {
            speak.Play("Speak");
            patchedUp.SetActive(true);
        }
    }
    public void Yap()
    {
        clear();
        speak.StopPlayback();
        speak.Play("Speak");
        talking.SetActive(true);

    }
    public void clear()
    {
        talking.SetActive(false);
    }
}
