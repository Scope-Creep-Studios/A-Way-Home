using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlatformerController : MonoBehaviour
{
    // Make a slot to hold the movement stats scriptable object.
    [SerializeField] private MovementStats _stats;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;

    private FrameInput _input;
    private Vector2 _velocity;
    private float _time;
    private bool _grounded;
    private bool _cachedQueriesStartInColliders;
    private bool _jumpToConsume = true;

    // A bundle for all of the inputs taken this frame.
    private struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public Vector2 Move;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        _cachedQueriesStartInColliders = Physics2D.queriesStartInColliders; // Found online; should help fix the walls counting as grounded issue.
    }

    private void Update()
    {
        _time += Time.deltaTime;
        ReadInput();
    }

    private void ReadInput()
    {
        _input = new FrameInput
        {
          JumpDown = Input.GetButtonDown("Jump"),
          JumpHeld = Input.GetButton("Jump"),
          Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
        };
        if (_input.JumpDown)
        {
            _jumpToConsume = true;
        }
    }

    private void FixedUpdate()
    {
        CheckCollisions();
        HandleJump();
        HandleDirection();
        HandleGravity();
        ApplyMovement();
    }

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false; // onlyreport what the raycast travels into, not what we already touch.
        bool groundHit = Physics2D.CapsuleCast(
            _col.bounds.center, _col.size, _col.direction, 0f, 
            Vector2.down, _stats.GroundColDistance, _stats.GroundLayers);
        if (!_grounded && groundHit)
        {
            _grounded = true;
        }
        else if (_grounded && !groundHit)
        {
            _grounded = false;
        }
        Physics2D.queriesStartInColliders = _cachedQueriesStartInColliders;
    }

    private void HandleJump()
    {
        if (!_jumpToConsume) return;
        if (_grounded) ExecuteJump();
        _jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        _velocity.y = _stats.JumpPower;
    }

    private void HandleDirection()
    {
        if (_input.Move.x == 0f)
        {
            float decel = _grounded
            ? _stats.GroundDeceleration : _stats.AirDeceleration;
            _velocity.x = Mathf.MoveTowards(_velocity.x, 0f, decel * Time.fixedDeltaTime);
        }
        else
        {
            float targetSpeed = _input.Move.x * _stats.MaxSpeed;
            _velocity.x = Mathf.MoveTowards(_velocity.x, targetSpeed, _stats.Acceleration * Time.fixedDeltaTime);
        }
    }

    private void HandleGravity()
    {
        if (_grounded && _velocity.y <= 0f)
        {
            _velocity.y = _stats.GroundingForce;
        }
        else
        {
            float gravity = _stats.FallAcceleration;
            _velocity.y = Mathf.MoveTowards(_velocity.y, -(_stats.MaxFallSpeed), gravity * Time.fixedDeltaTime);
        }
    }

    private void ApplyMovement() => _rb.velocity = _velocity;
}
