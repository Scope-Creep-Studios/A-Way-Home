using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlatformerController : MonoBehaviour
{
    // Make a slot to hold the movement stats scriptable object.
    [SerializeField] private MovementStats _stats;
    [SerializeField] private Keybinds _keys;
    
    // public things for the other scripts
    public bool HasControl = true;
    public Vector2 Velocity => _velocity;
    public bool Grounded => _grounded;
    public float MoveX => _input.X;
    public int Facing {get; private set;} = 1; // 1 means we are facing right, -1 means facing left.
    
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;

    private FrameInput _input;
    private Vector2 _velocity;
    private float _time;
    private float _timeLeftGround = float.MinValue;
    private float _timeJumpPressed = float.MinValue;
    private bool _grounded;
    private bool _cachedQueriesStartInColliders;
    private bool _jumpToConsume;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;

    // A bundle for all of the inputs taken this frame.
    private struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public float X;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        _cachedQueriesStartInColliders = Physics2D.queriesStartInColliders; // Found online; should help fix the walls counting as grounded issue.
    }

    // Reset the values that were stored before we swapped.
    private void OnEnable()
    {
        _velocity = _rb.velocity;
        _grounded = false;
        _coyoteUsable = false;
        _jumpToConsume = false;
        _endedJumpEarly = false;
    }

    private void Update()
    {
        _time += Time.deltaTime;
        ReadInput();
    }

    private void ReadInput()
    {
        if (!HasControl)
        {
            _input = default;
            _jumpToConsume = false;
            return;
        }

        float x = 0f;
        if (Input.GetKey(_keys.MoveRight)) x += 1f;
        if (Input.GetKey(_keys.MoveLeft)) x -= 1f;

        _input = new FrameInput
        {
          JumpDown = Input.GetKeyDown(_keys.Jump),
          JumpHeld = Input.GetKey(_keys.Jump),
          X = x
        };

        if (_input.X != 0f) Facing = (int)Mathf.Sign(_input.X);

        if (_input.JumpDown)
        {
            _jumpToConsume = true;
            _timeJumpPressed = _time;
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

        // ground and ceiling checks.
        bool groundHit = Physics2D.CapsuleCast(
            _col.bounds.center, _col.size, _col.direction, 0f, 
            Vector2.down, _stats.GroundColDistance, _stats.GroundLayers);
        bool ceilingHit = Physics2D.CapsuleCast(
            _col.bounds.center, _col.size, _col.direction, 0f,
            Vector2.up, _stats.GroundColDistance, _stats.GroundLayers);

        if (ceilingHit) _velocity.y = Mathf.Min(0f, _velocity.y);

        if (!_grounded && groundHit) // landed on this tick
        {
            _grounded = true;
            _endedJumpEarly = false;
            _coyoteUsable = true;
        }
        else if (_grounded && !groundHit) // left the ground on this tick
        {
            _grounded = false;
            _timeLeftGround = _time;
        }
        Physics2D.queriesStartInColliders = _cachedQueriesStartInColliders;
    }

    private bool HasBufferedJump => _time < _timeJumpPressed + _stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _timeLeftGround + _stats.CoyoteTime;

    private void HandleJump()
    {
        if (!_endedJumpEarly && !_grounded && !_input.JumpHeld && _velocity.y > 0f)
        {
            _endedJumpEarly = true;
        }
        if (!_jumpToConsume && !HasBufferedJump) return;
        if (_grounded || CanUseCoyote) ExecuteJump();
        _jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpPressed = float.MinValue;
        _coyoteUsable = false;
        _velocity.y = _stats.JumpPower;
    }

    private void HandleDirection()
    {
        if (_input.X == 0f)
        {
            float decel = _grounded
            ? _stats.GroundDeceleration : _stats.AirDeceleration;
            _velocity.x = Mathf.MoveTowards(_velocity.x, 0f, decel * Time.fixedDeltaTime);
        }
        else
        {
            float targetSpeed = _input.X * _stats.MaxSpeed;
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
            if (_endedJumpEarly && _velocity.y > 0f)
            {
                gravity *= _stats.EndJumpEarlyGravityModifier;
            }
            _velocity.y = Mathf.MoveTowards(_velocity.y, -(_stats.MaxFallSpeed), gravity * Time.fixedDeltaTime);
        }
    }

    private void ApplyMovement() => _rb.velocity = _velocity;
}
