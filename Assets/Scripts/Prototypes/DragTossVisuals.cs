using UnityEngine;

public class DragTossVisuals : MonoBehaviour
{
    [SerializeField] private CharacterSwap _swap;

    [Header("Colours")]
    [SerializeField] private Color _validColor = new Color(0.55f, 1f, 0.7f, 1f);
    [SerializeField] private Color _invalidColor = new Color(0.6f, 0.6f, 0.68f, 1f);

    [Header("Landing preview")]
    [SerializeField] private float _previewAlpha = 0.35f;

    [Header("Arc dots")]
    [SerializeField] private int _dotCount = 10;
    [SerializeField] private float _dotSize = 0.2f;
    [SerializeField] private float _dotDrift = 1f;
    [SerializeField] private float _dotAlpha = 0.8f;

    [Header("Hover highlight")]
    [SerializeField] private Color _highlightColor = new Color(0.85f, 1f, 0.9f, 1f);
    [SerializeField] private float _highlightAlpha = 0.35f;
    [SerializeField] private float _highlightPulse = 0.15f;
    [SerializeField] private float _pulseSpeed = 4f;
    [SerializeField] private float _fadeSpeed = 8f;

    private Transform _root;
    private Sprite _glowSprite;
    private Material _material;
    private SpriteRenderer _art;
    private SpriteRenderer _highlight;
    private SpriteRenderer _preview;
    private SpriteRenderer[] _dots;
    private float _glow;
    private float _dotPhase;

    private void Awake()
    {
        if (_swap == null) _swap = GetComponent<CharacterSwap>();
        _art = _swap.GhostPlatformArt;
        _glowSprite = MakeGlowSprite(64);
        _material = new Material(Shader.Find("Sprites/Default"));
        _root = new GameObject("DragTossFX").transform;

        int layer = _art != null ? _art.sortingLayerID : 0;
        int order = _art != null ? _art.sortingOrder : 0;

        _highlight = MakeSprite("GhostHighlight", null, layer, order + 1);
        _preview = MakeSprite("Preview", null, layer, order);
        _dots = new SpriteRenderer[Mathf.Max(_dotCount, 0)];
        for (int i = 0; i < _dots.Length; i++)
            _dots[i] = MakeSprite("Dot" + i, _glowSprite, layer, order + 1);
    }

    private void OnDisable()
    {
        _glow = 0f;
        Hide();
    }

    private void OnDestroy()
    {
        if (_root != null) Destroy(_root.gameObject);
        if (_material != null) Destroy(_material);
        if (_glowSprite != null)
        {
            Destroy(_glowSprite.texture);
            Destroy(_glowSprite);
        }
    }

    public void Render(bool hover, bool dragging, Vector2 mouse, Vector2 target, bool valid, Vector2 spot)
    {
        if (_root == null) return;
        DrawHighlight(hover || dragging);

        if (!dragging)
        {
            HideDrag();
            return;
        }

        Vector2 landing = valid ? spot : target;
        Color colour = valid ? _validColor : _invalidColor;
        DrawPreview(landing, colour);
        DrawDots(_swap.GhostPosition, landing, colour);
    }

    public void Hide()
    {
        if (_root == null) return;
        _highlight.enabled = false;
        HideDrag();
    }

    private void HideDrag()
    {
        _preview.enabled = false;
        for (int i = 0; i < _dots.Length; i++) _dots[i].enabled = false;
    }

    private void DrawHighlight(bool show)
    {
        _glow = Mathf.MoveTowards(_glow, show ? 1f : 0f, _fadeSpeed * Time.deltaTime);
        SpriteRenderer source = _art;
        _highlight.enabled = _glow > 0.001f && source != null && source.sprite != null && source.gameObject.activeInHierarchy;
        if (!_highlight.enabled) return;

        Transform from = source.transform;
        _highlight.sprite = source.sprite;
        _highlight.flipX = source.flipX;
        _highlight.flipY = source.flipY;
        _highlight.transform.SetPositionAndRotation(from.position, from.rotation);
        _highlight.transform.localScale = from.lossyScale;
        _highlight.sortingLayerID = source.sortingLayerID;
        _highlight.sortingOrder = source.sortingOrder + 1;

        float pulse = Mathf.Sin(Time.time * _pulseSpeed) * 0.5f + 0.5f;
        _highlight.color = WithAlpha(_highlightColor, (_highlightAlpha + _highlightPulse * pulse) * _glow);
    }

    private void DrawPreview(Vector2 landing, Color colour)
    {
        _preview.enabled = _art != null && _art.sprite != null;
        if (!_preview.enabled) return;

        Vector2 artOffset = (Vector2)_art.transform.position - _swap.GhostPosition;
        _preview.sprite = _art.sprite;
        _preview.flipX = _swap.PlatformFlipFor(_swap.TossFacingFor(landing));
        _preview.transform.position = landing + artOffset;
        _preview.transform.localScale = _art.transform.lossyScale;
        _preview.color = WithAlpha(colour, _previewAlpha);
    }

    private void DrawDots(Vector2 from, Vector2 to, Color colour)
    {
        if (_dots.Length == 0) return;
        _dotPhase = Mathf.Repeat(_dotPhase + Time.deltaTime * _dotDrift, 1f);

        for (int i = 0; i < _dots.Length; i++)
        {
            float progress = (i + _dotPhase) / _dots.Length;
            float fade = Mathf.Clamp01(progress / 0.15f) * Mathf.Clamp01((1f - progress) / 0.1f);

            SpriteRenderer dot = _dots[i];
            dot.enabled = fade > 0.001f;
            dot.transform.position = _swap.TossPathPoint(from, to, progress);
            dot.transform.localScale = new Vector3(_dotSize, _dotSize, 1f);
            dot.color = WithAlpha(colour, _dotAlpha * fade);
        }
    }

    private SpriteRenderer MakeSprite(string name, Sprite sprite, int layer, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(_root, false);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sharedMaterial = _material;
        sr.sortingLayerID = layer;
        sr.sortingOrder = order;
        sr.enabled = false;
        return sr;
    }

    private static Sprite MakeGlowSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];
        float half = (size - 1) * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - half) * (x - half) + (y - half) * (y - half)) / half;
                float a = Mathf.Clamp01(1f - d);
                pixels[y * size + x] = new Color(1f, 1f, 1f, a * a);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Color WithAlpha(Color c, float a)
    {
        c.a = Mathf.Clamp01(a);
        return c;
    }
}
