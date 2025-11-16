using UnityEngine;

public class DialogueNew : MonoBehaviour
{
    public GameObject zeroChoice;     // Main menu UI
    public GameObject choicePanel;    // Shared panel for Yap and Buy options
    public GameObject nevermind;      // Ends convo
    public GameObject yap;            // Yap convo
    public GameObject buy;            // Buy convo
    public GameObject anythingElse;   // Shown after yap
    public GameObject tooPoor;        // Message shown if cash <= 0

    public int cash = 0;              // Player's current cash

    private enum TalkStage { Start, Yap, Buy, End }
    private TalkStage talkStage = TalkStage.Start;

    private void Update()
    {
        switch (talkStage)
        {
            case TalkStage.Start:
                zeroChoice.SetActive(true);

                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2))
                {
                    // Both 1 and 2 go to Yap
                    Clear();
                    yap.SetActive(true);
                    choicePanel.SetActive(true);
                    talkStage = TalkStage.Yap;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    Clear();
                    buy.SetActive(true);
                    choicePanel.SetActive(true);
                    talkStage = TalkStage.Buy;
                }
                break;

            case TalkStage.Yap:
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    // Restart convo
                    Clear();
                    talkStage = TalkStage.Start;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    // End convo
                    Clear();
                    nevermind.SetActive(true);
                    talkStage = TalkStage.End;
                }
                break;

            case TalkStage.Buy:
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    Clear();
                    if (cash > 0)
                    {
                        // Proceed with buy logic here
                        Debug.Log("Purchase successful!");
                        // Optionally reduce cash or trigger item logic
                    }
                    else
                    {
                        tooPoor.SetActive(true);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    Clear();
                    nevermind.SetActive(true);
                    talkStage = TalkStage.End;
                }
                break;
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
}