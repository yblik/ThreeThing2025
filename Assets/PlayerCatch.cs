using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCatch : MonoBehaviour
{
    public int StroredCollectibles = 0;
    public GameObject currentTarget;
    public bool canCatch = false;

    public Text Display;

    public int Max = 5; //to be upgraded later

    private void Update()
    {
        Display.text = "Snakes:" + StroredCollectibles.ToString(); //spacing causes error with text alignment


        if (Input.GetMouseButton(0) && canCatch)
        {
            Debug.Log("Caught a collectible!");
            Destroy(currentTarget.gameObject);
            StroredCollectibles += 1;
            canCatch = false;
        }
    }
    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy") && StroredCollectibles <= Max)
        {
            currentTarget = other.gameObject;
            canCatch = true;
            Debug.Log(currentTarget);
        }
        //else
        //{
        //               canCatch = false;
        //    currentTarget = null;
        //    print ("Too many collectibles to catch more!");
        //}
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            canCatch = false;
            currentTarget = null;
        }
    }
}
