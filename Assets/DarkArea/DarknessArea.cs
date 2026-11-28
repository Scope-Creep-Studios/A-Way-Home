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
    private bool hasPlayedCameraHint = false;

    [Header("Camera Hint Settings")]
    public Camera mainCamera;
    public Transform switchCameraTarget;
    public float cameraMoveDuration = 1f;
    public float cameraHoldDuration = 1f;
    private CameraFollow cameraFollow;

    public Transform switchHintPosition;

    [Header("Switch Vision Shader")]
    public CatVision catVision;

    private MaterialPropertyBlock propertyBlock;
    private bool wasCatVisionEnabled;

    private static readonly int HoleCenterID =
        Shader.PropertyToID("_HoleCenter");

    private static readonly int RadiusID =
        Shader.PropertyToID("_Radius");

    private static readonly int SoftnessID =
        Shader.PropertyToID("_Softness");

    private static readonly int HoleActiveID =
        Shader.PropertyToID("_HoleActive");

    [Header("Switch Visibility")]
    public SpriteRenderer switchSprite;

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

    public void OnDialogueFinished()
    {
        if (hasPlayedCameraHint)
            return;

        hasPlayedCameraHint = true;
        StartCoroutine(ShowSwitchCameraHint());
    }

    private void ShowSwitchVision()
    {
        if (catVision == null || catVision.darknessRenderer == null)
            return;

        // Temporarily stop CatVision from updating the shader
        wasCatVisionEnabled = catVision.enabled;
        catVision.enabled = false;

        SpriteRenderer renderer = catVision.darknessRenderer;

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(propertyBlock);

        Vector2 center = switchHintPosition != null
            ? (Vector2)switchHintPosition.position
            : (Vector2)switchCameraTarget.position;

        propertyBlock.SetVector(
            HoleCenterID,
            new Vector4(center.x, center.y, 0f, 0f)
        );

        propertyBlock.SetFloat(RadiusID, catVision.holeRadius);
        propertyBlock.SetFloat(SoftnessID, catVision.edgeSoftness);
        propertyBlock.SetFloat(HoleActiveID, 1f);

        renderer.SetPropertyBlock(propertyBlock);
    }

    private void RestoreCatVision()
    {
        if (catVision != null && wasCatVisionEnabled)
        {
            catVision.enabled = true;
        }
    }

    private IEnumerator ShowSwitchCameraHint()
    {
        if (mainCamera == null || switchCameraTarget == null)
        {
            narrativeUI.narrativeBox.SetActive(false);
            narrativeUI.RestorePlayerControl();
            yield break;
        }
        ShowSwitchVision();
        if (switchSprite != null)
        {
            switchSprite.enabled = true;
        }

        cameraFollow = mainCamera.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.enabled = false;
        }
        
        Vector3 originalPosition = mainCamera.transform.position;

        Vector3 targetPosition = new Vector3(
            switchCameraTarget.position.x,
            switchCameraTarget.position.y,
            originalPosition.z
        );

        yield return MoveCamera(originalPosition, targetPosition);
        yield return new WaitForSeconds(cameraHoldDuration);
        yield return MoveCamera(
            mainCamera.transform.position,
            originalPosition
        );

        if (switchSprite != null)
        {
            switchSprite.enabled = false;
        }

        RestoreCatVision();
        // Camera hint finished
        if (cameraFollow != null)
        {
            cameraFollow.enabled = true;
        }
        narrativeUI.narrativeBox.SetActive(false);
        narrativeUI.RestorePlayerControl();
    }

    private IEnumerator MoveCamera(Vector3 startPosition, Vector3 targetPosition)
    {
        float elapsed = 0f;

        while (elapsed < cameraMoveDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / cameraMoveDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            mainCamera.transform.position =
                Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        mainCamera.transform.position = targetPosition;
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