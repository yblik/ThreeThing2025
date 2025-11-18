using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeTrap : MonoBehaviour
{
    private AIControllerScript snakeAI;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            snakeAI = other.GetComponent<AIControllerScript>();
            snakeAI.FreezeForCollection();
        }
    }
}
