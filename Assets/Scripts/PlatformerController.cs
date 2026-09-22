using System;
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
    }

    private void Update()
    {
        _time = Time.deltaTime;
        ReadInput();
    }

    private void ReadInput()
    {
        _input = new FrameInput
        {
          JumpDown = Input.GetButtonDown("Jump"),
          JumpHeld = Input.GetButton("Jump"),
          Move = new Vector2(Input.GetAxisRaw("horizontal"), Input.GetAxisRaw("Vertical"))
        };
    }

    private void FixedUpdate()
    {
        HandleDirection();
        ApplyMovement();
    }

    private void HandleDirection()
    {
        
    }

    private void ApplyMovement()
    {
        
    }
}
