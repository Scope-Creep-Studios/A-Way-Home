using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NarrativeUI : MonoBehaviour
{
    public GameObject narrativeBox;
    public GameObject girlDialogueBox;
    public GameObject spiritDialogueBox;

    public TMP_Text dialogueText;

    private DialogueSystem.DialogueEntry[] lines;
    private int currentLine;

    void Start()
    {
        narrativeBox.SetActive(false);
        girlDialogueBox.SetActive(false);
        spiritDialogueBox.SetActive(false);
    }

    public void ShowNarrative(DialogueSystem.DialogueEntry[] newLines)
    {
        lines = newLines;
        currentLine = 0;

        narrativeBox.SetActive(true);

        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        girlDialogueBox.SetActive(false);
        spiritDialogueBox.SetActive(false);

        dialogueText.text = lines[currentLine].Dialogue;

        if (lines[currentLine].Speaker == "Girl")
        {
            girlDialogueBox.SetActive(true);
        }
        else if (lines[currentLine].Speaker == "Ghost")
        {
            spiritDialogueBox.SetActive(true);
        }
    }

    void Update()
    {
        if (narrativeBox.activeSelf &&
            Input.GetKeyDown(KeyCode.E))
        {
            currentLine++;

            if (currentLine >= lines.Length)
            {
                narrativeBox.SetActive(false);
                girlDialogueBox.SetActive(false);
                spiritDialogueBox.SetActive(false);
            }
            else
            {
                ShowCurrentLine();
            }
        }
    }
}