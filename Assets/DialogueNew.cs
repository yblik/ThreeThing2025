using UnityEngine;

public class DialogueNew : MonoBehaviour
{
    public GameObject zeroChoice;     // Main menu UI
    public GameObject choicePanel;    // Optional follow-up UI
    public GameObject nevermind;      // Ends convo
    public GameObject yap;            // Yap response
    public GameObject buy;            // Buy response
    public GameObject anythingElse;   // Optional follow-up
    public GameObject tooPoor;        // Message if cash <= 0

    public Bank cash;                 // Player's current cash
    public int talkType = 0;          // Optional: 0 = phone, 1 = nurse, etc.

    private bool inDialogue = true;

    private void Update()
    {
        if (!inDialogue) return;

        zeroChoice.SetActive(true);

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // Nevermind
            Clear();
            nevermind.SetActive(true);
            EndDialogue();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // Yap
            Clear();
            yap.SetActive(true);
            choicePanel.SetActive(true); // Optional: show "anything else?" buttons
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // Buy
            Clear();
            if (cash != null && cash.Dinero > 0) //money
            {
                buy.SetActive(true);
                // Optionally deduct cash here
                // cash.amount -= itemCost;
            }
            else
            {
                tooPoor.SetActive(true);
            }
            choicePanel.SetActive(true); // Optional: show "anything else?" buttons
        }
    }

    public void Clear()
    {
        zeroChoice.SetActive(false);
        choicePanel.SetActive(false);

        nevermind.SetActive(false);
        yap.SetActive(false);
        buy.SetActive(false);

        anythingElse.SetActive(false);
        tooPoor.SetActive(false);
    }

    public void EndDialogue()
    {
        inDialogue = false;
        // Optionally disable this script or notify another system
    }

    // Optional: call this from a button to restart the menu
    public void RestartDialogue()
    {
        Clear();
        inDialogue = true;
    }
}