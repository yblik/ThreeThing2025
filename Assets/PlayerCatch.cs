using UnityEngine;
using UnityEngine.UI;

public class PlayerCatch : MonoBehaviour
{
    public int StoredCollectibles = 0;
    public GameObject currentTarget;
    public bool canCatch = false;

    public Text Display;
    public int Max = 5; // storage capacity (can be upgraded later)

    private void Update()
    {
        Display.text = "Snakes: " + StoredCollectibles.ToString();

        if (Input.GetMouseButtonDown(0) && canCatch && currentTarget != null)
        {
            // Confirm the target can actually be caught
            AIControllerScript ai = currentTarget.GetComponent<AIControllerScript>();
            if (ai != null && ai.currentState != AIControllerScript.AIState.Hiding)
            {
                Debug.Log("Caught a snake!");
                Destroy(currentTarget.gameObject);
                StoredCollectibles += 1;
                canCatch = false;
                currentTarget = null;
            }
            else
            {
                Debug.Log("Can't catch — the snake is hiding!");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        // Make sure the snake isn't hiding
        AIControllerScript ai = other.GetComponent<AIControllerScript>();
        if (ai != null && ai.currentState == AIControllerScript.AIState.Hiding)
        {
            canCatch = false;
            currentTarget = null;
            return;
        }

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

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            canCatch = false;
            currentTarget = null;
        }
    }
}
