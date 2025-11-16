using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    static int maxHealth = 10;

    public float currentHealth = maxHealth;

    public Image bar;

    public Animator TransitionOut;
    public Animator Player;

    public SwitchRoom roomSwitch;


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
            print("Player Dead");
            PlayerPrefs.SetFloat("HP", maxHealth);

            SpawnManager.Instance.SetRespawn(true); // Hospital spawn
            SpawnManager.Instance.SetPoint(1); // Hospital spawn

            //roomSwitch.roomNumber = 2; //off to medical
            roomSwitch.Muertes();
            sleep(1);


            TransitionOut.Play("SwitchRoom"); //get things into motion

            // Handle player death here
        }
    }
    public void sleep(int notalive)
    {
        Player.SetLayerWeight(0, 0);
        Player.SetLayerWeight(1, 0);
        Player.SetLayerWeight(2, 1);
        if (notalive == 0)
        {
            Player.SetBool("wakeUp", true);
        }
        else
        {
            Player.SetBool("dead", false);
        }
    }
    public void backinaction()
    {
        Player.SetLayerWeight(0, 1);
        Player.SetLayerWeight(1, 1);
        Player.SetLayerWeight(2, 0);
    }
}
