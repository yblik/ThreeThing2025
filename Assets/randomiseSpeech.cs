using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomiseSpeech : MonoBehaviour
{
    private int randomNumber;

    public GameObject one;
    public GameObject two;
    public GameObject three;

    private void OnEnable()
    {
        Clear();
        randomNumber = Random.Range(1, 4);
        if (randomNumber == 1)
        {
            one.SetActive(true);
        }
        else if (randomNumber == 2)
        {
            two.SetActive(true);
        }
        else if (randomNumber == 3)
        {
            three.SetActive(true);
        }
    }
    public void Clear()
    {
        one.SetActive(false);
        two.SetActive(false);
        three.SetActive(false);
    }
}
