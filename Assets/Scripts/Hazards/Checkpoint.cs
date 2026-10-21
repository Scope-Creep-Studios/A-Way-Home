using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
    [Tooltip("Where she reappears. Empty is this objects location")]
    [SerializeField] private Transform _spawnPoint;

    [Header("Look (optional)")]
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private Sprite _unlit;
    [SerializeField] private Sprite _lit;

    public UnityEvent Activated;   // For Audio @evan. Like a chime or smthn idk.

    public Vector2 SpawnPoint => _spawnPoint != null
        ? (Vector2)_spawnPoint.position
        : (Vector2)transform.position;

    private void Start() => ShowLit(false);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Respawn girl)) girl.SetCheckpoint(this);
    }

    public void Activate()
    {
        ShowLit(true);
        Activated.Invoke();
    }

    public void Deactivate() => ShowLit(false);

    private void ShowLit(bool lit)
    {
        if (_sprite == null) return;
        Sprite look = lit ? _lit : _unlit;
        if (look != null) _sprite.sprite = look;
    }
}