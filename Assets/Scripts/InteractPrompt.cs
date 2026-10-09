using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractPrompt : MonoBehaviour
{
    public TMP_Text text;

    void Update()
    {
        float t = (Mathf.Sin(Time.time * 2f) + 1f) / 2f;

        text.alpha = Mathf.Lerp(0.5f, 1f, t);

        float scale = Mathf.Lerp(0.9f, 1.05f, t);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}