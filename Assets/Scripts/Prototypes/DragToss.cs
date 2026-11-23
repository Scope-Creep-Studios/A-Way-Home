using UnityEngine;

public class DragToss : MonoBehaviour
{
    [SerializeField] private CharacterSwap _swap;
    [SerializeField] private KeyCode _button = KeyCode.Mouse0;
    [SerializeField] private KeyCode _cancelButton = KeyCode.Mouse1;
    [SerializeField] private float _grabRadius = 1.2f;
    [SerializeField] private bool _debugView;

    [Header("Debug look")]
    [SerializeField] private Material _lineMaterial;
    [SerializeField] private float _lineWidth = 0.06f;
    [SerializeField] private float _spiritLineWidth = 0.15f;
    [SerializeField] private Color _rangeColor = new Color(0.6f, 1f, 0.9f, 0.45f);
    [SerializeField] private Color _spiritColor = new Color(0.6f, 1f, 0.9f, 0.9f);
    [SerializeField] private Color _validColor = new Color(0.45f, 1f, 0.6f, 0.9f);
    [SerializeField] private Color _invalidColor = new Color(1f, 0.4f, 0.4f, 0.9f);

    private DragTossVisuals _visuals;
    private LineRenderer _range;
    private LineRenderer _grabHint;
    private LineRenderer _spiritLine;
    private LineRenderer _preview;
    private bool _dragging;

    private void Awake()
    {
        if (_swap == null) _swap = GetComponent<CharacterSwap>();
        _visuals = GetComponent<DragTossVisuals>();
        _range = TossVisuals.Create(transform, "TossRange", _lineMaterial, _lineWidth, 50);
        _grabHint = TossVisuals.Create(transform, "GrabHint", _lineMaterial, _lineWidth, 51);
        _spiritLine = TossVisuals.Create(transform, "SpiritLine", _lineMaterial, _spiritLineWidth, 52);
        _preview = TossVisuals.Create(transform, "TossPreview", _lineMaterial, _lineWidth, 53);
        _spiritLine.widthCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.35f));
    }

    private void OnDisable()
    {
        _dragging = false;
        HideDebug();
        if (_visuals != null) _visuals.Hide();
    }

    private void Update()
    {
        bool ready = _swap.CanToss;
        if (!ready) _dragging = false;

        Vector2 mouse = TossVisuals.MouseWorld();
        Vector2 ghost = _swap.GhostPosition;
        bool hover = ready && Vector2.Distance(mouse, ghost) <= _grabRadius;

        if (!_dragging)
        {
            if (hover && !TossVisuals.PointerOverUI() && Input.GetKeyDown(_button)) _dragging = true;
        }
        else if (Input.GetKeyDown(_cancelButton))
        {
            _dragging = false;
        }

        Vector2 target = _swap.ClampToTossRange(mouse);
        Vector2 spot = target;
        bool valid = _dragging && _swap.TryGetTossSpot(mouse, out spot);
        if (!valid) spot = target;

        bool released = _dragging && Input.GetKeyUp(_button);
        if (released) _dragging = false;

        if (!_debugView && _visuals != null)
        {
            HideDebug();
            _visuals.Render(hover, _dragging, mouse, target, valid, spot);
        }
        else
        {
            if (_visuals != null) _visuals.Hide();
            DrawDebug(hover, ghost, target, valid, spot);
        }

        if (released && valid) _swap.TryToss(mouse);
    }

    private void DrawDebug(bool hover, Vector2 ghost, Vector2 target, bool valid, Vector2 spot)
    {
        _range.enabled = _dragging || hover;
        if (_range.enabled)
        {
            TossVisuals.Circle(_range, _swap.GirlPosition, _swap.TossRange);
            Color c = _rangeColor;
            if (_dragging) c.a = Mathf.Min(1f, c.a * 1.8f);
            TossVisuals.SetColor(_range, c);
        }

        _grabHint.enabled = !_dragging && hover;
        if (_grabHint.enabled)
        {
            TossVisuals.Circle(_grabHint, ghost, _grabRadius * 0.6f, 32);
            TossVisuals.SetColor(_grabHint, _spiritColor);
        }

        _spiritLine.enabled = _dragging;
        _preview.enabled = _dragging;
        if (!_dragging) return;

        TossVisuals.Segment(_spiritLine, ghost, target);
        TossVisuals.SetColor(_spiritLine, valid ? _spiritColor : _invalidColor);
        TossVisuals.Box(_preview, spot + _swap.PlatformOffset, _swap.PlatformSize);
        TossVisuals.SetColor(_preview, valid ? _validColor : _invalidColor);
    }

    private void HideDebug()
    {
        if (_range != null) _range.enabled = false;
        if (_grabHint != null) _grabHint.enabled = false;
        if (_spiritLine != null) _spiritLine.enabled = false;
        if (_preview != null) _preview.enabled = false;
    }
}
