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
    public TMP_Text dialogueTextGhost;

    private DialogueSystem.DialogueEntry[] lines;
    private int currentLine;
    private bool waitForKeyRelease = false;

    public GameObject girl;
    public GameObject ghost;

    private PlatformerController girlController;
    private PlatformerController ghostController;

    private bool girlPreviousControl;
    private bool ghostPreviousControl;

    [SerializeField]
    private TMP_FontAsset GirlFont;

    [SerializeField]
    private TMP_FontAsset GhostFont;

    public MusicMuffler dialogueMuffler;

    void Start()
    {
        narrativeBox.SetActive(false);
        girlDialogueBox.SetActive(false);
        spiritDialogueBox.SetActive(false);

       if (girl != null)
        {
            girlController = girl.GetComponent<PlatformerController>();
        }

        if (ghost != null)
        {
            ghostController = ghost.GetComponent<PlatformerController>();
        }
    }

    public void ShowNarrative(DialogueSystem.DialogueEntry[] newLines)
    {
        if (newLines == null || newLines.Length == 0)
        {
            return;
        }

        lines = newLines;
        currentLine = 0;

        dialogueMuffler.SetFilter(true,1000);
        // Disable player control during dialogue
        if (girlController != null)
        {
            girlPreviousControl = girlController.HasControl;
            girlController.HasControl = false;
        }

        if (ghostController != null)
        {
            ghostPreviousControl = ghostController.HasControl;
            ghostController.HasControl = false;
        }

        narrativeBox.SetActive(true);
        ShowCurrentLine();
        waitForKeyRelease = true;
    }

    void ShowCurrentLine()
    {
        girlDialogueBox.SetActive(false);
        spiritDialogueBox.SetActive(false);

        if (lines[currentLine].Speaker == "Girl")
        {
            dialogueText.text = lines[currentLine].Dialogue;
            dialogueTextGhost.enabled = false;
            dialogueText.enabled = true;
            AudioController.Instance.ForcePlayRandom(SoundCategory.GirlDialogue);
            girlDialogueBox.SetActive(true);
            dialogueText.font = GirlFont;
        }
        else if (lines[currentLine].Speaker == "Ghost")
        {
            dialogueTextGhost.enabled = true;
            dialogueText.enabled = false;
            dialogueTextGhost.text = lines[currentLine].Dialogue;
            spiritDialogueBox.SetActive(true);
            dialogueTextGhost.font = GhostFont;
        }
    }

    void Update()
    {
        if (narrativeBox.activeSelf)
        {
            if (girlController != null)
            {
                girlController.HasControl = false;
            }

            if (ghostController != null)
            {
                ghostController.HasControl = false;
            }
        }
        else
        {
            return;
        }
        if (waitForKeyRelease)
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                waitForKeyRelease = false;
            }

            return;
        }

        // Press E to advance dialogue
        if (Input.GetKeyDown(KeyCode.E))
        {
            currentLine++;

            if (currentLine >= lines.Length)
            {
                
                dialogueMuffler.SetFilter(false,1000);
                // Close dialogue
                narrativeBox.SetActive(false);
                girlDialogueBox.SetActive(false);
                spiritDialogueBox.SetActive(false);

                // Restore previous player control state
                if (girlController != null)
                {
                    girlController.HasControl = girlPreviousControl;
                }

                if (ghostController != null)
                {
                    ghostController.HasControl = ghostPreviousControl;
                }
            }
            else
            {
                ShowCurrentLine();
            }
        }
    }
}