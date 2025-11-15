using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bank : MonoBehaviour
{
    public int Dinero;

    public Text Display;

    public bool bootlegVstart = false;

    private void Update()
    {
        if (!bootlegVstart)
        {
            Dinero = PlayerPrefs.GetInt("Money");
            bootlegVstart = true;
        }
        Display.text = "Cash: £" + Dinero.ToString(); //spacing causes error with text alignment
    }
    public void WorkdaysWorking(int amount)
    {
        Dinero += amount * 10; //make it seem like its worth more
    }
    public void SaveMoney()
    {
        PlayerPrefs.SetInt("Money", Dinero);
    }
}
