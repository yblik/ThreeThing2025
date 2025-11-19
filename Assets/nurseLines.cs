using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nurseLines : MonoBehaviour
{
    //added delay for speech so no more overlapping (hopefully)
    public Animator speak;
    public GameObject patchedUp;
    public GameObject talking;
    public float dialogueDuration = 3f;
    
    private bool isTalking = false;

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
        if (isTalking) return;

        isTalking = true;
        clear();
        speak.StopPlayback();
        speak.Play("Speak");
        talking.SetActive(true);
        Invoke("ResetTalking", dialogueDuration);
    }

    private void ResetTalking()
    {
        isTalking = false;
    }

    public void clear()
    {
        talking.SetActive(false);
    }
}
