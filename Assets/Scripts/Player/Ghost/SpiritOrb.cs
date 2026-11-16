using System;
using UnityEngine;

public class SpiritOrb : MonoBehaviour
{
    [SerializeField] private TrailRenderer _trail;

    public bool IsFlying { get; private set; }

    private Vector2 _from;
    private Func<Vector2> _to;
    private float _duration;
    private float _timeLeft;
    private Action _onArrive;

    private void Awake()
    {
        if (_trail == null) _trail = GetComponent<TrailRenderer>();
        if (!IsFlying) gameObject.SetActive(false);
    }

    public void Fly(Vector2 from, Func<Vector2> to, float duration, Action onArrive)
    {
        _from = from;
        _to = to;
        _duration = Mathf.Max(duration, 0.0001f);
        _timeLeft = _duration;
        _onArrive = onArrive;
        IsFlying = true;

        transform.position = new Vector3(from.x, from.y, transform.position.z);
        gameObject.SetActive(true);
        if (_trail != null) _trail.Clear();
    }

    public void Cancel()
    {
        IsFlying = false;
        _onArrive = null;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsFlying) return;

        _timeLeft -= Time.deltaTime;
        float t = 1f - Mathf.Clamp01(_timeLeft / _duration);
        float eased = t * t * (3f - 2f * t);
        Vector2 p = Vector2.Lerp(_from, _to(), eased);
        transform.position = new Vector3(p.x, p.y, transform.position.z);

        if (_timeLeft <= 0f)
        {
            Action done = _onArrive;
            IsFlying = false;
            _onArrive = null;
            gameObject.SetActive(false);
            done?.Invoke();
        }
    }
}
