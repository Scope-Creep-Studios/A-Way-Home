using UnityEngine;

public class InputBlocker : MonoBehaviour
{
    [SerializeField] private NarrativeUI _narrative;
    [SerializeField] private ScreenFader _fader;

    private static InputBlocker _instance;

    public static bool Blocked
    {
        get
        {
            if (PauseButton.paused) return true;
            if (_instance == null) return false;
            return _instance.IsTalking || _instance.IsFading;
        }
    }

    private bool IsTalking => _narrative != null && _narrative.narrativeBox.activeSelf;
    private bool IsFading => _fader != null && _fader.Busy;

    private void Awake() => _instance = this;

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}
