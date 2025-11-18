using UnityEngine;
using UnityEngine.UI;

public class PlayerCatch : MonoBehaviour
{
    public int StoredCollectibles = 0;
    public GameObject currentTarget;
    public bool canCatch = false;
    public bool Catching = false;

    public Text Display;
    public int Max = 5; // storage capacity (can be upgraded later)

    public Image CapacityBar;

    public bool bootlegVstart = false;

    private void Update()
    {
        if (!bootlegVstart)
        {
            StoredCollectibles = PlayerPrefs.GetInt("StoredSnakes");
            bootlegVstart = true;
        }
        Display.text = "Snakes: " + StoredCollectibles.ToString();
        CapacityBar.fillAmount = (float)StoredCollectibles / Max;

        if (Catching && canCatch && currentTarget != null)
        {
            // Confirm the target can actually be caught
                Debug.Log("Caught a snake!");
                Destroy(currentTarget.gameObject);
                StoredCollectibles += 1;
                canCatch = false;
                currentTarget = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;
        print("snake detected");

        // Make sure the snake isn't hiding

        // Normal catching logic
        if (StoredCollectibles < Max)
        {
            currentTarget = other.gameObject;
            canCatch = true;
        }
        else
        {
            canCatch = false;
            currentTarget = null;
        }
    }
    public void SaveSnakeAmount()
    {
        PlayerPrefs.SetInt("StoredSnakes", StoredCollectibles);

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            canCatch = false;
            currentTarget = null;
        }
    }
    public void StartCatching()
    {
        Catching = true;
    }
    public void StopCatching()
    {
        Catching = false;
    }
}
