using UnityEngine;

public class DragToss : MonoBehaviour
{
    [SerializeField] private CharacterSwap _swap;
    [SerializeField] private KeyCode _button = KeyCode.Mouse0;
    [SerializeField] private KeyCode _cancelButton = KeyCode.Mouse1;
    [SerializeField] private float _grabRadius = 1.2f;

    [Header("Look")]
    [SerializeField] private Material _lineMaterial;
    [SerializeField] private float _lineWidth = 0.06f;
    [SerializeField] private float _spiritLineWidth = 0.15f;
    [SerializeField] private Color _rangeColor = new Color(0.6f, 1f, 0.9f, 0.45f);
    [SerializeField] private Color _spiritColor = new Color(0.6f, 1f, 0.9f, 0.9f);
    [SerializeField] private Color _validColor = new Color(0.45f, 1f, 0.6f, 0.9f);
    [SerializeField] private Color _invalidColor = new Color(1f, 0.4f, 0.4f, 0.9f);

    private LineRenderer _range;
    private LineRenderer _grabHint;
    private LineRenderer _spiritLine;
    private LineRenderer _preview;
    private bool _dragging;

    private void Awake()
    {
        if (_swap == null) _swap = GetComponent<CharacterSwap>();
        _range = TossVisuals.Create(transform, "TossRange", _lineMaterial, _lineWidth, 50);
        _grabHint = TossVisuals.Create(transform, "GrabHint", _lineMaterial, _lineWidth, 51);
        _spiritLine = TossVisuals.Create(transform, "SpiritLine", _lineMaterial, _spiritLineWidth, 52);
        _preview = TossVisuals.Create(transform, "TossPreview", _lineMaterial, _lineWidth, 53);
        _spiritLine.widthCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.35f));
    }

    private void OnDisable()
    {
        _dragging = false;
        if (_range != null) _range.enabled = false;
        if (_grabHint != null) _grabHint.enabled = false;
        if (_spiritLine != null) _spiritLine.enabled = false;
        if (_preview != null) _preview.enabled = false;
    }

    private void Update()
    {
        bool ready = _swap.CanToss;
        if (!ready) _dragging = false;

        Vector2 mouse = TossVisuals.MouseWorld();
        Vector2 ghost = _swap.GhostPosition;
        bool nearGhost = Vector2.Distance(mouse, ghost) <= _grabRadius;

        _range.enabled = _dragging || (ready && nearGhost);
        if (_range.enabled)
        {
            TossVisuals.Circle(_range, _swap.GirlPosition, _swap.TossRange);
            Color c = _rangeColor;
            if (_dragging) c.a = Mathf.Min(1f, c.a * 1.8f);
            TossVisuals.SetColor(_range, c);
        }

        if (!_dragging)
        {
            _spiritLine.enabled = false;
            _preview.enabled = false;

            _grabHint.enabled = ready && nearGhost;
            if (_grabHint.enabled)
            {
                TossVisuals.Circle(_grabHint, ghost, _grabRadius * 0.6f, 32);
                TossVisuals.SetColor(_grabHint, _spiritColor);
            }

            if (ready && nearGhost && !TossVisuals.PointerOverUI() && Input.GetKeyDown(_button))
                _dragging = true;
            return;
        }

        _grabHint.enabled = false;

        if (Input.GetKeyDown(_cancelButton))
        {
            _dragging = false;
            _spiritLine.enabled = false;
            _preview.enabled = false;
            return;
        }

        Vector2 target = _swap.ClampToTossRange(mouse);
        bool valid = _swap.TryGetTossSpot(mouse, out Vector2 spot);

        _spiritLine.enabled = true;
        TossVisuals.Segment(_spiritLine, ghost, target);
        TossVisuals.SetColor(_spiritLine, valid ? _spiritColor : _invalidColor);

        _preview.enabled = true;
        TossVisuals.Box(_preview, (valid ? spot : target) + _swap.PlatformOffset, _swap.PlatformSize);
        TossVisuals.SetColor(_preview, valid ? _validColor : _invalidColor);

        if (Input.GetKeyUp(_button))
        {
            _dragging = false;
            _spiritLine.enabled = false;
            _preview.enabled = false;
            if (valid) _swap.TryToss(mouse);
        }
    }
}
