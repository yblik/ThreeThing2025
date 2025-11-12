using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCatch : MonoBehaviour
{
    public int StroredCollectibles = 0;
    public GameObject currentTarget;
    public bool canCatch = false;

    private void Update()
    {
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
        if (other.CompareTag("Enemy"))
        {
            currentTarget = other.gameObject;
            canCatch = true;
            Debug.Log(currentTarget);
        }
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
