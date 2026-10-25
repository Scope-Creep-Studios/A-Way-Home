using UnityEngine;
using UnityEngine.Events;

public class SpiritTree : Interactable
{
    [Header("Spirit Tree")]
    [SerializeField] private Collider2D _solid;
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private float _fadeTime = 1f;
    [SerializeField] private bool _startsLocked;

    public UnityEvent Unlocked;
    public UnityEvent Dispelled;

    public bool IsLocked { get; private set; }

    private enum Phase { Standing, Fading, Gone }
    private Phase _phase = Phase.Standing;
    private float _timeLeft;

    public override bool CanInteract => !IsLocked && _phase == Phase.Standing;

    protected override void Awake()
    {
        base.Awake();
        IsLocked = _startsLocked;
    }

    public void Unlock()
    {
        if (!IsLocked) return;
        IsLocked = false;
        Unlocked?.Invoke();
    }

    protected override void OnInteract(Interactor who)
    {
        _phase = Phase.Fading;
        _timeLeft = _fadeTime;
    }

    private void Update()
    {
        if (_phase != Phase.Fading) return;

        _timeLeft -= Time.deltaTime;
        float alpha = _fadeTime > 0f ? Mathf.Clamp01(_timeLeft / _fadeTime) : 0f;
        SetAlpha(alpha);

        if (_timeLeft <= 0f) Finish();
    }

    private void Finish()
    {
        _phase = Phase.Gone;
        if (_solid != null) _solid.enabled = false;
        Dispelled?.Invoke();
        gameObject.SetActive(false);
    }

    private void SetAlpha(float a)
    {
        if (_sprite == null) return;
        Color c = _sprite.color;
        c.a = a;
        _sprite.color = c;
    }
}
