using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DarknessArea : MonoBehaviour
{
    [Header("Darkness Settings")]
    public SpriteRenderer darknessRenderer;
    public float fadeDuration = 1.5f;

    private bool isLit = false;

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