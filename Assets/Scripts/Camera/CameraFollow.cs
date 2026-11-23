using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 1f, -10f);
    [SerializeField] private float _smoothTime = 0.15f;

    [Header("Look ahead")]
    [SerializeField] private float _lookAhead = 1.5f;
    [SerializeField] private float _lookAheadSmooth = 0.35f;

    [Header("Vertical dead zone")]
    [SerializeField] private float _deadZoneUp = 1.5f;
    [SerializeField] private float _deadZoneDown = 2.5f;
    [SerializeField] private float _groundRecenterSpeed = 10f;

    [Header("Vertical limits")]
    [SerializeField] private Transform _artRoot;
    [SerializeField] private float _margin = 0.5f;

    private Vector3 _velocity;
    private Camera _cam;
    private bool _hasLimits;
    private float _bottom;
    private float _top;

    private PlatformerController _targetBody;
    private float _lookOffset;
    private float _lookVelocity;
    private float _lookDir = 1f;
    private float _focusY;
    private bool _hasFocus;

    private void Awake() => _cam = GetComponent<Camera>();

    private void Start()
    {
        _hasLimits = TryMeasureArt(out _bottom, out _top);
        SetTarget(_target);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        _targetBody = target != null ? target.GetComponent<PlatformerController>() : null;
    }

    private void LateUpdate()
    {
        if (_target == null) return;
        Vector3 raw = _target.position + _offset;

        float lookTarget = 0f;
        if (_targetBody != null && _lookAhead > 0f)
        {
            if (Mathf.Abs(_targetBody.Velocity.x) > 0.5f) _lookDir = Mathf.Sign(_targetBody.Velocity.x);
            lookTarget = _lookDir * _lookAhead;
        }
        _lookOffset = Mathf.SmoothDamp(_lookOffset, lookTarget, ref _lookVelocity, _lookAheadSmooth);

        Vector3 goal = new Vector3(raw.x + _lookOffset, FocusY(raw.y), raw.z);
        if (_hasLimits) goal.y = ClampY(goal.y);
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref _velocity, _smoothTime);
    }

    private float FocusY(float targetY)
    {
        if (!_hasFocus)
        {
            _focusY = targetY;
            _hasFocus = true;
        }

        if (targetY > _focusY + _deadZoneUp) _focusY = targetY - _deadZoneUp;
        else if (targetY < _focusY - _deadZoneDown) _focusY = targetY + _deadZoneDown;

        if (_targetBody == null || _targetBody.Grounded)
            _focusY = Mathf.MoveTowards(_focusY, targetY, _groundRecenterSpeed * Time.deltaTime);

        return _focusY;
    }

    private float ClampY(float y)
    {
        float halfHeight = _cam.orthographicSize;
        float min = _bottom + halfHeight;
        float max = _top - halfHeight;
        if (min > max) return (_bottom + _top) * 0.5f;
        return Mathf.Clamp(y, min, max);
    }

    private bool TryMeasureArt(out float bottom, out float top)
    {
        bottom = 0f;
        top = 0f;
        if (_artRoot == null) return false;

        SpriteRenderer[] sprites = _artRoot.GetComponentsInChildren<SpriteRenderer>();
        if (sprites.Length == 0) return false;

        bottom = float.MaxValue;
        top = float.MinValue;
        foreach (SpriteRenderer sprite in sprites)
        {
            Bounds b = sprite.bounds;
            bottom = Mathf.Min(bottom, b.min.y);
            top = Mathf.Max(top, b.max.y);
        }

        bottom += _margin;
        top -= _margin;
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (!TryMeasureArt(out float bottom, out float top)) return;

        Gizmos.color = Color.yellow;
        float x = transform.position.x;
        Gizmos.DrawLine(new Vector3(x - 50f, bottom), new Vector3(x + 50f, bottom));
        Gizmos.DrawLine(new Vector3(x - 50f, top), new Vector3(x + 50f, top));
    }
}
