using UnityEngine;

public class ClickToss : MonoBehaviour
{
    [SerializeField] private CharacterSwap _swap;
    [SerializeField] private KeyCode _button = KeyCode.Mouse0;
    [SerializeField] private bool _showPreview = true;

    [Header("Look")]
    [SerializeField] private Material _lineMaterial;
    [SerializeField] private float _lineWidth = 0.06f;
    [SerializeField] private Color _rangeColor = new Color(0.6f, 1f, 0.9f, 0.45f);
    [SerializeField] private Color _validColor = new Color(0.45f, 1f, 0.6f, 0.9f);
    [SerializeField] private Color _invalidColor = new Color(1f, 0.4f, 0.4f, 0.9f);

    private LineRenderer _range;
    private LineRenderer _preview;

    private void Awake()
    {
        if (_swap == null) _swap = GetComponent<CharacterSwap>();
        _range = TossVisuals.Create(transform, "TossRange", _lineMaterial, _lineWidth, 50);
        _preview = TossVisuals.Create(transform, "TossPreview", _lineMaterial, _lineWidth, 51);
    }

    private void OnDisable()
    {
        if (_range != null) _range.enabled = false;
        if (_preview != null) _preview.enabled = false;
    }

    private void Update()
    {
        bool ready = _swap.CanToss;
        bool overUI = TossVisuals.PointerOverUI();
        Vector2 mouse = TossVisuals.MouseWorld();

        _range.enabled = _swap.HasGhost;
        if (_range.enabled)
        {
            TossVisuals.Circle(_range, _swap.GirlPosition, _swap.TossRange);
            Color c = _rangeColor;
            if (!ready) c.a *= 0.35f;
            TossVisuals.SetColor(_range, c);
        }

        Vector2 spot = default;
        bool valid = ready && _swap.TryGetTossSpot(mouse, out spot);
        _preview.enabled = _showPreview && ready && !overUI;
        if (_preview.enabled)
        {
            Vector2 at = valid ? spot : _swap.ClampToTossRange(mouse);
            TossVisuals.Box(_preview, at + _swap.PlatformOffset, _swap.PlatformSize);
            TossVisuals.SetColor(_preview, valid ? _validColor : _invalidColor);
        }

        if (ready && !overUI && Input.GetKeyDown(_button)) _swap.TryToss(mouse);
    }
}
