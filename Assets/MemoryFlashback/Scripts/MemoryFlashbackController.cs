using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MemoryFlashbackController : MonoBehaviour
{
    [Header("Data")]
    public TextAsset memoryCSV;

    [Header("UI")]
    public TMP_Text dialogueText;
    public TMP_Text nameText;

    [Header("Portraits")]
    public Image girlPortrait;
    public Image dadPortrait;

    [Header("Background")]
    public Image backgroundImage;
    public Sprite bg01LivingRoom;
    public Sprite bg02Bedroom;
    public Sprite bg03EntranceHall;

    [Header("Fonts")]
    public TMP_FontAsset girlFont;
    public TMP_FontAsset dadFont;
    public TMP_FontAsset narrationFont;

    public event Action Finished;

    private readonly List<MemoryLine> lines = new List<MemoryLine>();
    private int currentLine = 0;
    private bool finished = false;
    private bool inputEnabled = true;

    private class MemoryLine
    {
        public string Speaker;
        public string DisplayName;
        public string Dialogue;
        public string Background;
        public string GirlState;
        public string DadState;
    }

    private void Start()
    {
        LoadCSV();

        if (lines.Count == 0)
        {
            Debug.LogError("MemoryFlashback.csv has no readable dialogue lines.");
            return;
        }

        currentLine = 0;
        ShowCurrentLine();
    }

    private void Update()
    {
        if (lines.Count == 0)
            return;

        if (!inputEnabled)
            return;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.E) ||
            Input.GetMouseButtonDown(0))
        {
            NextLine();
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    private void NextLine()
    {
        if (currentLine >= lines.Count - 1)
        {
            Debug.Log("Memory flashback test finished.");

            if (!finished)
            {
                finished = true;
                Finished?.Invoke();
            }

            return;
        }

        currentLine++;
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        MemoryLine line = lines[currentLine];

        dialogueText.text = line.Dialogue;

        // Narration has no displayed name.
        if (string.IsNullOrWhiteSpace(line.DisplayName))
        {
            nameText.gameObject.SetActive(false);
        }
        else
        {
            nameText.gameObject.SetActive(true);
            nameText.text = line.DisplayName;
        }

        ApplyTextStyle(line);

        ApplyPortraitState(girlPortrait, line.GirlState);
        ApplyPortraitState(dadPortrait, line.DadState);

        ApplyBackground(line.Background);

        Debug.Log(
            $"Line {currentLine + 1}: " +
            $"Speaker={line.Speaker}, " +
            $"Background={line.Background}, " +
            $"Girl={line.GirlState}, " +
            $"Dad={line.DadState}"
        );
    }

    private void ApplyPortraitState(Image portrait, string state)
    {
        if (portrait == null)
            return;

        switch (state)
        {
            case "Hidden":
                portrait.gameObject.SetActive(false);
                break;

            case "Bright":
                portrait.gameObject.SetActive(true);
                portrait.color = Color.white;
                break;

            case "Dim":
                portrait.gameObject.SetActive(true);
                portrait.color = new Color(0.35f, 0.35f, 0.35f, 1f);
                break;

            default:
                Debug.LogWarning($"Unknown portrait state: {state}");
                break;
        }
    }

    private void ApplyTextStyle(MemoryLine line)
    {
        switch (line.Speaker)
        {
            case "Girl":
                if (girlFont != null)
                {
                    dialogueText.font = girlFont;
                    nameText.font = girlFont;
                }

                dialogueText.fontSize = 31f;
                dialogueText.color = new Color32(255, 209, 246, 255);
                nameText.color = Color.white;
                break;

            case "Dad":
                if (dadFont != null)
                {
                    dialogueText.font = dadFont;
                    nameText.font = dadFont;
                }

                dialogueText.fontSize = 31f;
                dialogueText.color = new Color32(176, 240, 192, 255);
                nameText.color = Color.white;
                break;

            case "Narration":
                if (narrationFont != null)
                {
                    dialogueText.font = narrationFont;
                }

                dialogueText.fontSize = 31f;
                dialogueText.color = Color.white;
                break;

            default:
                Debug.LogWarning($"Unknown speaker for text style: {line.Speaker}");
                break;
        }
    }

    private void ApplyBackground(string background)
    {
        if (backgroundImage == null)
            return;

        if (string.IsNullOrWhiteSpace(background))
            return;

        switch (background)
        {
            case "BLACK":
                backgroundImage.sprite = null;
                backgroundImage.color = Color.black;
                break;

            case "BG01":
                backgroundImage.sprite = bg01LivingRoom;
                backgroundImage.color = Color.white;
                break;

            case "BG02":
                backgroundImage.sprite = bg02Bedroom;
                backgroundImage.color = Color.white;
                break;

            case "BG03":
                backgroundImage.sprite = bg03EntranceHall;
                backgroundImage.color = Color.white;
                break;

            default:
                Debug.LogWarning($"Unknown background: {background}");
                break;
        }
    }

    private void LoadCSV()
    {
        lines.Clear();

        if (memoryCSV == null)
        {
            Debug.LogError("Memory CSV has not been assigned.");
            return;
        }

        string[] rows = memoryCSV.text
            .Replace("\r", "")
            .Split('\n');

        // Row 0 is the header.
        for (int i = 1; i < rows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(rows[i]))
                continue;

            List<string> columns = ParseCSVLine(rows[i]);

            if (columns.Count < 6)
            {
                Debug.LogWarning($"Skipping invalid CSV row {i + 1}: {rows[i]}");
                continue;
            }

            MemoryLine line = new MemoryLine
            {
                Speaker = columns[0].Trim(),
                DisplayName = columns[1].Trim(),
                Dialogue = columns[2].Trim(),
                Background = columns[3].Trim(),
                GirlState = columns[4].Trim(),
                DadState = columns[5].Trim()
            };

            lines.Add(line);
        }
    }

    private List<string> ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        StringBuilder current = new StringBuilder();

        bool insideQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // Two quotation marks inside a quoted field = literal quote.
                if (insideQuotes &&
                    i + 1 < line.Length &&
                    line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }
            }
            else if (c == ',' && !insideQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());

        return result;
    }
}