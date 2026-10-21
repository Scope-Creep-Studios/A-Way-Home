using System;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup _group;
    [SerializeField] private float _fadeOutTime = 0.4f;
    [SerializeField] private float _holdTime = 0.2f;   
    [SerializeField] private float _fadeInTime = 0.4f;

    private enum Phase { Idle, FadingOut, Holding, FadingIn }

    private Phase _phase = Phase.Idle;
    private float _timeLeft;   
    private float _duration;     
    private Action _atBlack;     
    private Action _whenDone;    

    public bool Busy => _phase != Phase.Idle;

    private void Awake()
    {
        _group.alpha = 0f;
        _group.blocksRaycasts = false; 
    }

    public void Play(Action atBlack, Action whenDone = null)
    {
        if (Busy) return;
        _atBlack = atBlack;
        _whenDone = whenDone;
        Enter(Phase.FadingOut, _fadeOutTime);
    }

    private void Update()
    {
        if (_phase == Phase.Idle) return;

        _timeLeft -= Time.deltaTime;
        float progress = 1f - Mathf.Clamp01(_timeLeft / _duration);
        bool phaseDone = _timeLeft <= 0f;

        switch (_phase)
        {
            case Phase.FadingOut:
                _group.alpha = progress;
                if (phaseDone)
                {
                    _group.alpha = 1f;
                    _atBlack?.Invoke();             
                    Enter(Phase.Holding, _holdTime);
                }
                break;

            case Phase.Holding:
                if (phaseDone) Enter(Phase.FadingIn, _fadeInTime);
                break;

            case Phase.FadingIn:
                _group.alpha = 1f - progress;
                if (phaseDone)
                {
                    _group.alpha = 0f;
                    Enter(Phase.Idle, 0f);         
                    _whenDone?.Invoke();
                }
                break;
        }
    }

    private void Enter(Phase phase, float duration)
    {
        _phase = phase;
        _duration = Mathf.Max(duration, 0.0001f); 
        _timeLeft = _duration;
    }
}