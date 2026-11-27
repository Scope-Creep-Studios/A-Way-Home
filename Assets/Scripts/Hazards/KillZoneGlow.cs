using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KillZoneGlow : MonoBehaviour
{
    [Header("Glow")]
    [SerializeField] private Color _color = new Color(0.65f, 0.3f, 1f, 0.6f);
    [SerializeField] private float _height = 5f;
    [SerializeField, Range(0f, 1f)] private float _solidHeight = 0f;
    [SerializeField] private float _fadeSharpness = 1.6f;
    [SerializeField] private float _extraWidth = 0.3f;
    [SerializeField] private float _pulseSpeed = 0.5f;
    [SerializeField] private float _pulseStrength = 0.2f;

    [Header("Particles")]
    [SerializeField] private bool _showParticles = true;
    [SerializeField] private float _particleAmount = 0.5f;
    [SerializeField] private int _maxParticles = 30;
    [SerializeField] private float _particleSize = 0.25f;
    [SerializeField] private float _particleSpeedMin = 0.4f;
    [SerializeField] private float _particleSpeedMax = 0.9f;
    [SerializeField] private float _particleWobble = 0.15f;
    [SerializeField] private float _particleOpacity = 0.7f;

    [Header("Sorting")]
    [SerializeField] private string _sortingLayer = "InteriorWalls";
    [SerializeField] private int _sortingOrder = 1;

    private struct Wisp
    {
        public SpriteRenderer Renderer;
        public float X;
        public float Speed;
        public float Age;
        public float Life;
        public float Phase;
    }

    private static readonly Dictionary<Vector2Int, Sprite> _gradients = new Dictionary<Vector2Int, Sprite>();
    private static Sprite _dotSprite;
    private static Material _material;

    private SpriteRenderer _glow;
    private Wisp[] _wispList = new Wisp[0];
    private Bounds _area;
    private float _pulseOffset;

    private void Start()
    {
        EnsureShared();
        _area = GetComponent<BoxCollider2D>().bounds;
        _pulseOffset = Random.value * 10f;
        int layer = SortingLayer.NameToID(_sortingLayer);

        _glow = MakeRenderer("Glow", GetGradient(_solidHeight, _fadeSharpness), layer, _sortingOrder);
        _glow.transform.position = new Vector3(_area.center.x, _area.min.y, transform.position.z);
        SetWorldScale(_glow.transform, new Vector2(_area.size.x + _extraWidth * 2f, _height));

        if (!_showParticles) return;
        int count = Mathf.Clamp(Mathf.CeilToInt(_area.size.x * _particleAmount), 1, Mathf.Max(_maxParticles, 1));
        _wispList = new Wisp[count];
        for (int i = 0; i < count; i++)
        {
            _wispList[i].Renderer = MakeRenderer("Wisp" + i, _dotSprite, layer, _sortingOrder + 1);
            Respawn(ref _wispList[i]);
            _wispList[i].Age = Random.Range(0f, _wispList[i].Life);
        }
    }

    private void Update()
    {
        if (_glow == null) return;

        float pulse = 1f + Mathf.Sin((Time.time + _pulseOffset) * _pulseSpeed * Mathf.PI * 2f) * _pulseStrength;
        Color glow = _color;
        glow.a = Mathf.Clamp01(_color.a * pulse);
        _glow.color = glow;

        Color wispColor = Color.Lerp(_color, Color.white, 0.3f);
        for (int i = 0; i < _wispList.Length; i++)
        {
            ref Wisp w = ref _wispList[i];
            w.Age += Time.deltaTime;
            if (w.Age >= w.Life) Respawn(ref w);

            float t = w.Age / w.Life;
            float x = w.X + Mathf.Sin(w.Phase + w.Age * 2f) * _particleWobble;
            float y = _area.min.y + w.Speed * w.Age;
            w.Renderer.transform.position = new Vector3(x, y, transform.position.z);

            float size = _particleSize * (1f - 0.5f * t);
            SetWorldScale(w.Renderer.transform, new Vector2(size, size));

            wispColor.a = Mathf.Sin(t * Mathf.PI) * _particleOpacity;
            w.Renderer.color = wispColor;
        }
    }

// Borrowed from forum; this is so we can see a rough preview in the scene view.
    private void OnDrawGizmos()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) return;

        Bounds area = box.bounds;
        float width = area.size.x + _extraWidth * 2f;
        const int bands = 8;
        float bandHeight = _height / bands;

        for (int i = 0; i < bands; i++)
        {
            float fade = Fade((i + 0.5f) / bands, _solidHeight, _fadeSharpness);
            Color c = _color;
            c.a = _color.a * fade * 0.6f;
            Gizmos.color = c;
            Vector3 centre = new Vector3(area.center.x, area.min.y + bandHeight * (i + 0.5f), transform.position.z);
            Gizmos.DrawCube(centre, new Vector3(width, bandHeight, 0f));
        }
    }

    private void Respawn(ref Wisp w)
    {
        w.X = Random.Range(_area.min.x, _area.max.x);
        w.Speed = Random.Range(_particleSpeedMin, _particleSpeedMax);
        w.Life = Mathf.Max(_height / Mathf.Max(w.Speed, 0.01f) * Random.Range(0.6f, 1f), 0.1f);
        w.Age = 0f;
        w.Phase = Random.Range(0f, Mathf.PI * 2f);
    }

    private SpriteRenderer MakeRenderer(string name, Sprite sprite, int layer, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sharedMaterial = _material;
        sr.sortingLayerID = layer;
        sr.sortingOrder = order;
        return sr;
    }

    private static void SetWorldScale(Transform t, Vector2 size)
    {
        Vector3 parent = t.parent != null ? t.parent.lossyScale : Vector3.one;
        float px = Mathf.Abs(parent.x) > 0.0001f ? parent.x : 1f;
        float py = Mathf.Abs(parent.y) > 0.0001f ? parent.y : 1f;
        t.localScale = new Vector3(size.x / px, size.y / py, 1f);
    }

    private static void EnsureShared()
    {
        if (_material == null) _material = new Material(Shader.Find("Sprites/Default"));
        if (_dotSprite == null) _dotSprite = MakeDot(32);
    }

    private static float Fade(float height, float solid, float sharpness)
    {
        if (height <= solid) return 1f;
        if (solid >= 1f) return 1f;
        float t = (height - solid) / (1f - solid);
        return Mathf.Pow(1f - Mathf.Clamp01(t), Mathf.Max(sharpness, 0.01f));
    }

    private static Sprite GetGradient(float solid, float sharpness)
    {
        Vector2Int key = new Vector2Int(Mathf.RoundToInt(solid * 100f), Mathf.RoundToInt(sharpness * 100f));
        if (_gradients.TryGetValue(key, out Sprite sprite) && sprite != null) return sprite;
        sprite = MakeGradient(64, key.x / 100f, key.y / 100f);
        _gradients[key] = sprite;
        return sprite;
    }

    private static Sprite MakeGradient(int size, float solid, float sharpness)
    {
        Texture2D tex = NewTexture(size);
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            float v = y / (size - 1f);
            float vertical = Fade(v, solid, sharpness);
            for (int x = 0; x < size; x++)
            {
                float u = x / (size - 1f);
                float edge = Mathf.Clamp01(Mathf.Min(u, 1f - u) / 0.12f);
                edge = edge * edge * (3f - 2f * edge);
                pixels[y * size + x] = new Color(1f, 1f, 1f, vertical * edge);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0f), size);
    }

    private static Sprite MakeDot(int size)
    {
        Texture2D tex = NewTexture(size);
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

    private static Texture2D NewTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }
}
