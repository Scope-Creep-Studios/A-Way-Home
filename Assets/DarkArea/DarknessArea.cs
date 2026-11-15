using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DarknessArea : MonoBehaviour
{
    [Header("Darkness Settings")]
    public SpriteRenderer darknessRenderer;
    public float fadeDuration = 1.5f;

    [Header("Dialogue Settings")]
    public NarrativeUI narrativeUI;
    public DialogueSystem dialogueSystem;

    public string dialogueType = "Narrative";
    public string dialogueName = "DarknessArea";
    public string dialogueState = "FirstEntry";

    private bool isLit = false;
    private bool hasTriggeredDialogue = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggeredDialogue)
            return;

        if (other.CompareTag("Player"))
        {
            DialogueSystem.DialogueEntry[] dialogue =
                dialogueSystem.GetDialogue(dialogueType, dialogueName, dialogueState);

            if (dialogue.Length > 0)
            {
                hasTriggeredDialogue = true;
                narrativeUI.ShowNarrative(dialogue);
            }
        }
    }

    public void LightUp()
    {
        if (isLit)
            return;

        isLit = true;
        StartCoroutine(FadeDarkness());
    }

    private IEnumerator FadeDarkness()
    {
        if (darknessRenderer == null)
            yield break;

        Color color = darknessRenderer.color;
        float startAlpha = color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / fadeDuration);
            color.a = Mathf.Lerp(startAlpha, 0f, t);

            darknessRenderer.color = color;

            yield return null;
        }

        color.a = 0f;
        darknessRenderer.color = color;
        darknessRenderer.enabled = false;
    }
}