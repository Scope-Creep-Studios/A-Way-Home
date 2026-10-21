using System;
using UnityEngine;

[RequireComponent(typeof(PlatformerController))]
public class Respawn : MonoBehaviour
{
    [SerializeField] private ScreenFader _fader;   

    
    public event Action Respawned;
    public bool IsRespawning { get; private set; }

    private PlatformerController _controller;
    private Checkpoint _current;
    private Vector2 _startPoint;

    public Vector2 Point => _current != null ? _current.SpawnPoint : _startPoint;

    private void Awake()
    {
        _controller = GetComponent<PlatformerController>();
        _startPoint = transform.position;   
    }

    public void SetCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == _current) return;  
        if (_current != null) _current.Deactivate();
        _current = checkpoint;
        _current.Activate();
    }

    public void Die()
    {
        if (IsRespawning) return;  

        IsRespawning = true;
        if (_fader != null)
        {
            _fader.Play(ReturnToCheckpoint, () => IsRespawning = false);
        }
        else  
        {
            ReturnToCheckpoint();
            IsRespawning = false;
        }
    }

    private void ReturnToCheckpoint()
    {
        _controller.Teleport(Point);
        Respawned?.Invoke();
    }
}
