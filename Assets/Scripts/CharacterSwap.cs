using System.Collections;
using UnityEngine;

public class CharacterSwap : MonoBehaviour
{
    [SerializeField] private Character _girl;
    [SerializeField] private Character _ghost;
    [SerializeField] private PlatformerController _girlController;
    [SerializeField] private CompanionFollow _ghostFollow;
    [SerializeField] private CameraFollow _cam;
    [SerializeField] private Keybinds _keys;

    [Header("Ghost swap flight from shoulder")]
    [SerializeField] private float _deployTime = 0.25f;
    [SerializeField] private Vector2 _deployOffset = new Vector2(1.4f, 0f);

    [Header("Cooldown after platform expiry")]
    [SerializeField] private float _swapCooldown = 1f;

    private Character _active;
    private CharState _ghostPrev = CharState.Following;

    private bool _deploying;
    private float _deployLeft; // nnednto count down while ghost is deploying from shoulder area.
    private Vector2 _deployFrom;
    private Vector2 _deployTo;
    
    private float _cooldownLeft; // counmt down after the platform expires.

    private void OnEnable() => _ghost.StateChanged += OnGhostStateChanged;
    private void OnDisable() => _ghost.StateChanged -= OnGhostStateChanged;

    private void Start()
    {
        _ghost.Enter(CharState.Following);
        GiveControlTo(_girl);
    }

    private void Update()
    {
        if (_cooldownLeft > 0f) _cooldownLeft -= Time.deltaTime;

        if (_deploying)
        {
            TickDeploy();
            return;
        }

        if (!Input.GetKeyDown(_keys.Swap)) return;

        if (_active == _girl) TryStartDeploy();
        else SwapBackToGirl();
    }

    private void TryStartDeploy()
    {
        if (_cooldownLeft > 0f) return;
        if(_ghost.State == CharState.Platform) return;

        _girl.Enter(CharState.Idle);
        _ghost.Enter(CharState.Deploying);

        _deployFrom = _ghost.transform.position;
        Vector2 offset = _deployOffset;
        offset.x *= _girlController.Facing;
        _deployTo = (Vector2)_girl.transform.position + offset;

        _deployLeft = _deployTime;
        _deploying = true;
        if (_cam != null) _cam.SetTarget(_ghost.transform);
    }

    private void TickDeploy()
    {
        _deployLeft -= Time.deltaTime;

        float t = _deployTime <= 0f ? 1f : (1f - Mathf.Max(_deployLeft, 0f) / _deployTime);
        _ghost.transform.position = Vector2.Lerp(_deployFrom, _deployTo, t);

        if (_deployLeft <= 0f)
        {
            _deploying = false;
            GiveControlTo(_ghost);
        }
    }

    private void SwapBackToGirl()
    {
        _ghost.Enter(CharState.Following);
        GiveControlTo(_girl);
    }

    private void GiveControlTo(Character c)
    {
        _active = c;
        c.Enter(CharState.Controlled);
        if (_cam != null) _cam.SetTarget(c.transform);
    }

    private void OnGhostStateChanged(Character ghost, CharState s)
    {
        // When ghost freezes, pass back to girl to control.
        if (s == CharState.Platform &&  _active == _ghost) GiveControlTo(_girl);

        // Platform expired snap back to girl and begin the cooldown.
        if (_ghostPrev == CharState.Platform && s == CharState.Following)
        {
            _ghostFollow.Snap();
            _cooldownLeft = _swapCooldown;
        }
        _ghostPrev = s;
    }
}
    