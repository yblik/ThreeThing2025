using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bank : MonoBehaviour
{
    public int Dinero;

    public Text Display;

    private void Update()
    {
        Display.text = "Cash:£" + Dinero.ToString(); //spacing causes error with text alignment
    }
    public void WorkdaysWorking(int amount)
    {
        Dinero += amount * 10; //make it seem like its worth more
    }
}
