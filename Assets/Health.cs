using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int maxHealth = 10;

    private float currentHealth;

    public Image bar;

    void Start()
    {
        currentHealth = PlayerPrefs.GetFloat("HP");
        if (currentHealth == 0)
        {
            currentHealth = maxHealth; //lying in bed
        }
    }
    public bool bootlegVstart = false;

    private void Update()
    {
        if (!bootlegVstart)
        {
            if (currentHealth == 0)
            {
                currentHealth = maxHealth; //lying in bed
            }

            currentHealth = PlayerPrefs.GetFloat("HP");
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
