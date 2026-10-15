using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSystem : MonoBehaviour
{
    public TextAsset DialogueCSV;

    [System.Serializable]
    public class DialogueEntry
    {
        public string Type;
        public string Name;
        public string State;
        public string Speaker;
        public string Dialogue;
    }

    private List<DialogueEntry> dialogueList = new List<DialogueEntry>();

    void Start()
    {
        ParseDialogueText();
    }

    void ParseDialogueText()
    {
        string[] lines = DialogueCSV.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] splitRow = lines[i].Split(',');

            if (splitRow.Length >= 5)
            {
                DialogueEntry entry = new DialogueEntry
                {
                    Type = splitRow[0].Trim(),
                    Name = splitRow[1].Trim(),
                    State = splitRow[2].Trim(),
                    Speaker = splitRow[3].Trim(),
                    Dialogue = string.Join(",", splitRow, 4, splitRow.Length - 4).Trim()
                };

                dialogueList.Add(entry);
            }
        }
    }

    public DialogueEntry[] GetDialogue(string type, string name, string state)
    {
        List<DialogueEntry> dialogues = new List<DialogueEntry>();

        foreach (DialogueEntry entry in dialogueList)
        {
            if (entry.Type == type &&
                entry.Name == name &&
                entry.State == state)
            {
                dialogues.Add(entry);
            }
        }

        return dialogues.ToArray();
    }
}
