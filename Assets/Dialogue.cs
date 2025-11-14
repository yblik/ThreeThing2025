using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//public class Dialogue : MonoBehaviour
//{
//    public Text dialogueText;

//    // NO idea what Im doing here but trying to make it modular - you got to finish this maks

//    //should display text in bits so everything to be displ;ayed in text should be printed (maybe get string list and add to new list while displaying it in update so it = tostring() on the dialogueText)
//    //There should be one dialogue per whenever in dialogue so no multipe dialogues after each other

//    //use input to make and test multiple choices and Ill try pick up from there since I'll add buying logic - buenas suerte maks no probs dude

//    private void Start()
//    {
//        DialogueLibarary diaglib = new DialogueLibarary();
//        //dialogueText.text = diaglib.;
//    }
//}

public class Dialogue : MonoBehaviour
{
    public Text dialogueText;
    public Text speakerText;
    public GameObject dialoguePanel;
    public Button continueButton;

    private List<DialoguePart> currentDialogue;
    private int index = 0;
    private bool isTyping = false;
    private float typeSpeed = 0.03f;

    private void Start()
    {
        dialoguePanel.SetActive(false);
        continueButton.onClick.AddListener(NextLine);
    }

    public void StartDialogue(string dialogueID)
    {
        currentDialogue = DialogueLibrary.GetDialogue(dialogueID);

        if (currentDialogue.Count == 0)
        {
            Debug.LogWarning("Dialogue ID not found: " + dialogueID);
            return;
        }

        dialoguePanel.SetActive(true);
        index = 0;
        ShowLine();
    }

    private void ShowLine()
    {
        DialoguePart part = currentDialogue[index];

        speakerText.text = part.speaker;
        dialogueText.text = "";

        StopAllCoroutines();
        StartCoroutine(TypeLine(part.text));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }

    private void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentDialogue[index].text;
            isTyping = false;
            return;
        }

        index++;

        if (index < currentDialogue.Count)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        index = 0;
    }
}

//class DialogueLibarary
//{
//    /// <summary>
//    /// example system to get dialogue parts by id
//    /// </summary>
//    /// <param name="dialogueId"></param>
//    /// <returns></returns>
//    public static List<DialoguePart> GetDialogue(string dialogueId)
//    {
//        if (dialogueId == "intro")
//        {
//            return new List<DialoguePart>
//            {
//                new DialoguePart("Alice: ", "Welcome to our adventure!"),
//                new DialoguePart("Bob: ", "I'm excited to get started."),
//                new DialoguePart("Alice: ", "Let's go explore the unknown!")
//            };
//        }
//        else if (dialogueId == "farewell")
//        {
//            return new List<DialoguePart>
//            {
//                new DialoguePart("Alice: ", "It's time to say goodbye."),
//                new DialoguePart("Bob: ", "I'll miss you!"),
//                new DialoguePart("Alice: ", "Until we meet again!")
//            };
//        }
//        else
//        {
//            return new List<DialoguePart>();
//        }
//    }
//}
//class DialoguePart
//{
//    public string speaker;
//    public string text;
//    public DialoguePart(string speaker, string text)
//    {
//        this.speaker = speaker;
//        this.text = text;
//    }
//}

class DialogueLibrary
{
    public static Dictionary<string, List<DialoguePart>> dialogueDict =
        new Dictionary<string, List<DialoguePart>>
        {
            {
                "intro", new List<DialoguePart>
                {
                    new DialoguePart("Alice", "Welcome to our adventure!"),
                    new DialoguePart("Bob", "I'm excited to get started."),
                    new DialoguePart("Alice", "Let's go explore the unknown!")
                }
            },

            {
                "shopkeeper", new List<DialoguePart>
                {
                    new DialoguePart("Trader", "Welcome! Here to trade snakes?"),
                    new DialoguePart("Player", "Yeah, what do you have?"),
                    new DialoguePart("Trader", "Take a look. Prices change daily.")
                }
            },

            {
                "bush_tutorial", new List<DialoguePart>
                {
                    new DialoguePart("Guide", "Sometimes snakes hide in bushes."),
                    new DialoguePart("Guide", "Listen carefully for rustling."),
                    new DialoguePart("Guide", "Move slowly to avoid spooking them.")
                }
            }
        };

    public static List<DialoguePart> GetDialogue(string id)
    {
        if (dialogueDict.ContainsKey(id))
            return dialogueDict[id];

        return new List<DialoguePart>();
    }
}

class DialoguePart
{
    public string speaker;
    public string text;

    public DialoguePart(string speaker, string text)
    {
        this.speaker = speaker;
        this.text = text;
    }
}
