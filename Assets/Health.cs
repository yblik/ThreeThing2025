using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    static int maxHealth = 10;

    public float currentHealth = maxHealth;

    public Image bar;

   
    public bool bootlegVstart = false;

    private void Update()
    {
        if (!bootlegVstart)
        {
            currentHealth = PlayerPrefs.GetFloat("HP");
            if (currentHealth == 0)
            {
                currentHealth = maxHealth; //lying in bed
            }

            bootlegVstart = true;
        }
        bar.fillAmount = currentHealth / maxHealth;
    }
    public void SaveHealth()
    {
        PlayerPrefs.SetFloat("HP", currentHealth);

    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            PlayerPrefs.SetFloat("HP", maxHealth);
            // Handle player death here
        }
    }
}
