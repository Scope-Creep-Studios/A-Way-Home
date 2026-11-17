using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 1f, -10f);
    [SerializeField] private float _smoothTime = 0.15f;

    [Header("Vertical limits")]
    [SerializeField] private Transform _artRoot;
    [SerializeField] private float _margin = 0.5f;

    private Vector3 _velocity;
    private Camera _cam;
    private bool _hasLimits;
    private float _bottom;
    private float _top;

    private void Awake() => _cam = GetComponent<Camera>();

    private void Start() => _hasLimits = TryMeasureArt(out _bottom, out _top);

    public void SetTarget(Transform target) => _target = target;

    private void LateUpdate()
    {
        if (_target == null) return;
        Vector3 goal = _target.position + _offset;
        if (_hasLimits) goal.y = ClampY(goal.y);
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref _velocity, _smoothTime);
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
