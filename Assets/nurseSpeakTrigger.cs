using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nurseSpeakTrigger : MonoBehaviour
{
    public nurseLines NL;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            NL.Yap();
        }
    }
}
