using UnityEngine;
using TMPro;

public class InteractPrompt : MonoBehaviour
{
    public TMP_Text text;
    public float flashSpeed = 1.5f;
    public float minAlpha = 0.45f;
    public float maxAlpha = 1f;

    void Update()
    {
        if (text == null) return;

        float t = Mathf.PingPong(Time.time * flashSpeed, 1f);
        text.alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
    }
}