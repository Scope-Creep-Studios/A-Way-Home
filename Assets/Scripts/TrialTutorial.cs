using UnityEngine;
using TMPro;

public class TutorialHint : MonoBehaviour
{
    public TextMeshPro text;

    public bool showOnStart = false;
    public float fadeSpeed = 1f;
    public float flashSpeed = 4f;
    public float flashMinAlpha = 0.3f;
    private bool showing = false;
    private float fade = 0f;

    private void Start(){
        if (text == null){
            text = GetComponent<TextMeshPro>();
        }
        text.alpha = 0f;
        showing = showOnStart;
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player") || other.CompareTag("Ghost")){
            showing = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other){
        if (other.CompareTag("Player") || other.CompareTag("Ghost")){
            showing = false;
        }
    }

    private void Update(){
        if (showing){
            fade = Mathf.Clamp01(fade + fadeSpeed * Time.deltaTime);
        }
        else{
            fade = Mathf.Clamp01(fade - fadeSpeed * Time.deltaTime);
        }
        float flash = Mathf.Lerp(flashMinAlpha, 1f, (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f);
        text.alpha = fade * flash;
    }
}