using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shopBrowse : MonoBehaviour
{

    public ShopkeeperSpeech SS;
    public int itemID;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            SS.Yap(itemID);
        }
    }
}
