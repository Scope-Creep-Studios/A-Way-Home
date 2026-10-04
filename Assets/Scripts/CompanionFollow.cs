using System;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class CompanionFollow : MonoBehaviour
{
    [SerializeField] private PlatformerController _leader; // girl is leader
    [SerializeField] private Vector2 _offset = new Vector2(-1.2f, 1.2f);
    [SerializeField] private float _smoothTime = 0.12f;
    [SerializeField] private float _bobHeight = 0.15f;
    [SerializeField] private float _bobSpeed = 3f;

    private Character _character;
    private Vector2 _smoothVelocity; // for storing the SmoothDamp value. 
    
    public Vector2 ShoulderPosition
    {
        get
        {
            Vector2 offset = _offset;
            offset.x *= _leader.Facing; // stays behind us when we switch directions as the girl.
            return (Vector2)_leader.transform.position + offset;
        }
    }

    private void Awake() => _character = GetComponent<Character>();

    private void LateUpdate()
    {
        if (_character.State != CharState.Following) return;

        Vector2 goal = ShoulderPosition;
        goal.y += Mathf.Sin(Time.time * _bobSpeed) * _bobHeight; // slight float.
        transform.position = Vector2.SmoothDamp(transform.position, goal, ref _smoothVelocity, _smoothTime);
    }
    
    public void Snap()
    {
        transform.position = ShoulderPosition;
        _smoothVelocity = Vector2.zero;
    }
}
