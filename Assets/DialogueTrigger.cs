using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public string dialogueID = "shopkeeper";
    private Dialogue dialogueManager;

    private void Start()
    {
        dialogueManager = FindObjectOfType<Dialogue>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            dialogueManager.StartDialogue(dialogueID);
        }
    }
}

