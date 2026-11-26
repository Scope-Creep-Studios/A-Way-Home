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
    public bool CanJumpNow => _grounded || CanUseCoyote;
    public int Facing {get; private set;} = 1; // 1 means we are facing right, -1 means facing left.
    public float SpeedPercent => _stats.MaxSpeed > 0f ? Mathf.Abs(_velocity.x) / _stats.MaxSpeed : 0f;
    
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
    private bool _hadControl;
    private bool _jumpLocked;

    private const float Skin = 0.01f;
    private const float AssistStep = 0.05f;

    // A bundle for all of the inputs taken this frame.
    private struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public float X;
    }

    public void Teleport(Vector2 position)
    {
        transform.position = position;
        _rb.position = position;
        _rb.velocity = Vector2.zero;
        _velocity = Vector2.zero;
        _grounded = false; // need to re-detect the ground or weird shit happens.
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
        _hadControl = false;
    }

    private void Update()
    {
        _time += Time.deltaTime;
        ReadInput();
    }

    private void ReadInput()
    {
        bool canRead = HasControl && !InputBlocker.Blocked;
        if (canRead && !_hadControl) _jumpLocked = Input.GetKey(_keys.Jump);
        _hadControl = canRead;

        if (!canRead)
        {
            _input = default;
            _jumpToConsume = false;
            return;
        }

        bool jumpKey = Input.GetKey(_keys.Jump);
        if (_jumpLocked && !jumpKey) _jumpLocked = false;

        float x = 0f;
        if (Input.GetKey(_keys.MoveRight)) x += 1f;
        if (Input.GetKey(_keys.MoveLeft)) x -= 1f;

        _input = new FrameInput
        {
          JumpDown = !_jumpLocked && Input.GetKeyDown(_keys.Jump),
          JumpHeld = !_jumpLocked && jumpKey,
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
        TryLedgeAssist();
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

        if (ceilingHit && !TryCornerCorrect()) _velocity.y = Mathf.Min(0f, _velocity.y);

        if (!_grounded && groundHit) // landed on this tick
        {
            //play if character landed on ground from sufficient height (so it doesn't play when jumping from lower to higher platforom);
            AudioController.Instance.StopAfter(SoundCategory.Falling,0.15f);
            if(_velocity.y<=-10){AudioController.Instance.PlayRandom(SoundCategory.Land);}
            _grounded = true;
            _endedJumpEarly = false;
            _coyoteUsable = true;
        }
        else if (_grounded && !groundHit) // left the ground on this tick
        {
            if (!TrySnapToGround())
            {
                //play footstep if character slips off ledge
                if(_velocity.y<=1f){AudioController.Instance.Play(SoundCategory.Footsteps,1);}
                _grounded = false;
                _timeLeftGround = _time;
            }
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
            float accel = _stats.Acceleration;
            bool turning = Mathf.Abs(_velocity.x) > 0.01f && Mathf.Sign(_velocity.x) != Mathf.Sign(_input.X);
            if (turning) accel *= _stats.TurnAccelerationMultiplier;
            _velocity.x = Mathf.MoveTowards(_velocity.x, targetSpeed, accel * Time.fixedDeltaTime);
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
            else if (_velocity.y < 0f)
            {
                gravity *= _stats.FallGravityMultiplier;
            }
            _velocity.y = Mathf.MoveTowards(_velocity.y, -(_stats.MaxFallSpeed), gravity * Time.fixedDeltaTime);
        }
    }

    private void ApplyMovement() => _rb.velocity = _velocity;

    private bool BlockedAt(Vector2 offset)
    {
        return Physics2D.OverlapCapsule((Vector2)_col.bounds.center + offset, _col.size, _col.direction, 0f,
            _stats.GroundLayers) != null;
    }

    private bool CastFrom(Vector2 offset, Vector2 dir, float distance)
    {
        return Physics2D.CapsuleCast((Vector2)_col.bounds.center + offset, _col.size, _col.direction, 0f,
            dir, distance, _stats.GroundLayers);
    }

    private bool TrySnapToGround()
    {
        if (!_stats.GroundSnap || _velocity.y > 0.01f) return false;

        RaycastHit2D hit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f,
            Vector2.down, _stats.GroundSnapDistance, _stats.GroundLayers);
        if (!hit) return false;

        _rb.position += Vector2.down * Mathf.Max(0f, hit.distance - Skin);
        return true;
    }

    private bool TryCornerCorrect()
    {
        if (!_stats.CornerCorrection || _velocity.y <= 0f) return false;

        float probe = _velocity.y * Time.fixedDeltaTime + _stats.GroundColDistance;
        int first = _input.X < 0f ? -1 : 1;

        for (float d = AssistStep; d <= _stats.CornerCorrectionWidth + 0.0001f; d += AssistStep)
        {
            for (int k = 0; k < 2; k++)
            {
                int side = k == 0 ? first : -first;
                Vector2 shift = Vector2.right * (side * d);
                if (BlockedAt(shift) || CastFrom(Vector2.zero, Vector2.right * side, d)) continue;
                if (CastFrom(shift, Vector2.up, probe)) continue;

                _rb.position += shift;
                return true;
            }
        }
        return false;
    }

    private void TryLedgeAssist()
    {
        if (!_stats.LedgeAssist || _input.X == 0f) return;
        if (_velocity.y > _stats.LedgeAssistMaxRiseSpeed) return;

        Vector2 dir = Vector2.right * Mathf.Sign(_input.X);
        float probe = Mathf.Abs(_velocity.x) * Time.fixedDeltaTime + _stats.GroundColDistance;

        bool cached = Physics2D.queriesStartInColliders;
        Physics2D.queriesStartInColliders = false;

        if (CastFrom(Vector2.zero, dir, probe))
        {
            for (float up = AssistStep; up <= _stats.LedgeAssistHeight + 0.0001f; up += AssistStep)
            {
                Vector2 lift = Vector2.up * up;
                if (BlockedAt(lift) || CastFrom(Vector2.zero, Vector2.up, up)) break;
                if (CastFrom(lift, dir, probe)) continue;

                _rb.position += lift;
                if (_velocity.y < 0f) _velocity.y = 0f;
                break;
            }
        }

        Physics2D.queriesStartInColliders = cached;
    }

}
