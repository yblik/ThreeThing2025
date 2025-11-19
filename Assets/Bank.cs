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
    //for black jack: 
    public void AddMoney(int amount)
    {
        Dinero += amount;
        PlayerPrefs.SetInt("Money", Dinero);
    }

    public void RemoveMoney(int amount)
    {
        Dinero -= amount;
        if (Dinero < 0) Dinero = 0; // prevent negatives

        PlayerPrefs.SetInt("Money", Dinero);
    }

    public int GetMoney()
    {
        return Dinero;
    }
}
