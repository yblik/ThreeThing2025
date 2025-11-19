using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopkeeperSpeech : MonoBehaviour
{
    public Animator speak;

    public GameObject item0;


    public GameObject item1;
    public GameObject item2;
    public GameObject item3;

    public void Awake()
    {
        speak.Play("Speak");
    }
    public void Yap(int ID)
    {
        clear();
        speak.StopPlayback();
        speak.Play("Speak");
        switch (ID)
        {
            case 0:
                item1.SetActive(true); break;
            case 1:
                item2.SetActive(true); break;
            case 2:
                item3.SetActive(true); break;

        }

    }
    public void clear()
    {
        item0.SetActive(false);
        item1.SetActive(false);
        item2.SetActive(false);
        item3.SetActive(false);
    }
}
