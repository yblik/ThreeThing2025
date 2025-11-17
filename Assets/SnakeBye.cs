using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeBye : MonoBehaviour
{
    public GameObject snake;
    public GameObject SnakeDust;
    public Transform pointOfDust;

    public void bye()
    {
        snake.SetActive(false);
    }
    public void Dust()
    {
        Instantiate(SnakeDust, pointOfDust.position, pointOfDust.rotation);
    }
}
