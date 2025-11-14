using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    public Text dialogueText;

    // NO idea what Im doing here but trying to make it modular - you got to finish this maks

    //should display text in bits so everything to be displ;ayed in text should be printed (maybe get string list and add to new list while displaying it in update so it = tostring() on the dialogueText)
    //There should be one dialogue per whenever in dialogue so no multipe dialogues after each other

    //use input to make and test multiple choices and Ill try pick up from there since I'll add buying logic - buenas suerte maks

    private void Start()
    {
        DialogueLibarary diaglib = new DialogueLibarary();
        //dialogueText.text = diaglib.;
    }
}
class DialogueLibarary
{
    /// <summary>
    /// example system to get dialogue parts by id
    /// </summary>
    /// <param name="dialogueId"></param>
    /// <returns></returns>
    public static List<DialoguePart> GetDialogue(string dialogueId)
    {
        if (dialogueId == "intro")
        {
            return new List<DialoguePart>
            {
                new DialoguePart("Alice: ", "Welcome to our adventure!"),
                new DialoguePart("Bob: ", "I'm excited to get started."),
                new DialoguePart("Alice: ", "Let's go explore the unknown!")
            };
        }
        else if (dialogueId == "farewell")
        {
            return new List<DialoguePart>
            {
                new DialoguePart("Alice: ", "It's time to say goodbye."),
                new DialoguePart("Bob: ", "I'll miss you!"),
                new DialoguePart("Alice: ", "Until we meet again!")
            };
        }
        else
        {
            return new List<DialoguePart>();
        }
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
