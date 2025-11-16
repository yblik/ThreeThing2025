using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public Health Health;
    public bool damagePlayer;
    private void Update()
    {
        if (damagePlayer)
        {
            Health.TakeDamage(0.01f);
            print("Trap Damage");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            damagePlayer = true;
            //Health = other.GetComponent<Health>();
            print ("Player in Trap");
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            damagePlayer = false;
        }
    }
}
