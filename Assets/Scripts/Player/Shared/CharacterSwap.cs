using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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

    [Header("Spirit orb")]
    [SerializeField] private SpiritOrb _orb;
    [SerializeField] private float _returnTime = 0.25f;

    [Header("Toss (prototype)")]
    [SerializeField] private float _tossRange = 3f;
    [SerializeField] private float _tossTime = 0.35f;
    [SerializeField] private float _tossArc = 0.6f;
    [SerializeField] private int _maxPlatformsPerTrip = 2;
    [SerializeField] private bool _requireInitialPlatform;

    [Header("Ghost unlock")]
    [SerializeField] private bool _startsWithGhost = true;
    public UnityEvent GhostJoined;
    public UnityEvent GhostRemoved;

    [Header("Cooldown after platform expiry")]
    [SerializeField] private float _swapCooldown = 1f;

    [Header("UI Objects")]
    [SerializeField] private GameObject _girlUIActive;
    [SerializeField] private GameObject _ghostUIActive;
    [SerializeField] private GameObject _girlUIPassive;
    [SerializeField] private GameObject _ghostUIPassive;
    [SerializeField] private GameObject spiritTimer1;
    [SerializeField] private GameObject spiritTimer2;


    [SerializeField] private Respawn _girlRespawn;
    [SerializeField] private NarrativeUI narrativeUI;

    private SpriteRenderer _ghostUIPassiveSpriteRenderer;
    private PlatformAbility _ghostPlatform;
    private CapsuleCollider2D _ghostBody;
    private PlatformerController _ghostController;
    private GhostAnimator _ghostAnimator;
    private CapsuleCollider2D _girlBody;
    private readonly RaycastHit2D[] _deployHits = new RaycastHit2D[8];
    private readonly Collider2D[] _deployOverlaps = new Collider2D[8];
    private const float DeployStep = 0.05f;

    private Character _active;
    private bool _inTransit;
    private bool _hasGhost;
    private bool _joining;

    public bool HasGhost => _hasGhost;

    private int _controlChangedFrame = -1;
    private int _transition;
    private int _chainCount;

    private bool _tossing;
    private Vector2 _tossFrom;
    private Vector2 _tossTo;
    private float _tossDuration;
    private float _tossLeft;
    private int _tossFacing;

    public float TossRange => _tossRange;
    public Vector2 GirlPosition => _girl.transform.position;
    public Vector2 GhostPosition => _ghost.transform.position;
    public Vector2 PlatformSize => _ghostPlatform.PlatformSize;
    public Vector2 PlatformOffset => _ghostPlatform.PlatformOffset;

    public bool CanToss =>
        _hasGhost && !_inTransit && _active == _girl &&
        Time.frameCount != _controlChangedFrame &&
        !InputBlocker.Blocked &&
        ((_ghost.State == CharState.Platform && _chainCount < _maxPlatformsPerTrip) ||
         (_ghost.State == CharState.Following && _cooldownLeft <= 0f && !_requireInitialPlatform));

    public Vector2 ClampToTossRange(Vector2 point)
    {
        Vector2 girl = GirlPosition;
        return girl + Vector2.ClampMagnitude(point - girl, _tossRange);
    }

    public bool TryGetTossSpot(Vector2 point, out Vector2 spot)
    {
        return _ghostPlatform.TryFindRoomAt(ClampToTossRange(point), out spot);
    }

    public bool TryToss(Vector2 point)
    {
        if (!CanToss) return false;
        if (!TryGetTossSpot(point, out Vector2 spot)) return false;

        _transition++;
        _chainCount++;

        _tossFrom = _ghost.transform.position;
        _tossTo = spot;
        _tossDuration = Mathf.Max(_tossTime, 0.0001f);
        _tossLeft = _tossDuration;

        float dx = _tossTo.x - _tossFrom.x;
        int direction = Mathf.Abs(dx) > 0.05f ? (int)Mathf.Sign(dx) : 0;
        _tossFacing = direction != 0 ? direction : _girlController.Facing;

        _ghost.Enter(CharState.Deploying);
        _ghostAnimator.SetSpinning(true, _tossFacing);
        _inTransit = true;
        _tossing = true;
        return true;
    }

    private void TickToss()
    {
        _tossLeft -= Time.deltaTime;
        float t = 1f - Mathf.Clamp01(_tossLeft / _tossDuration);
        float eased = t * t * (3f - 2f * t);
        Vector2 p = Vector2.Lerp(_tossFrom, _tossTo, eased);
        p += Vector2.up * (_tossArc * 4f * eased * (1f - eased));
        _ghost.transform.position = new Vector3(p.x, p.y, _ghost.transform.position.z);

        if (_tossLeft > 0f) return;

        StopToss();
        _inTransit = false;
        _ghostPlatform.BecomePlatformAt(_tossTo, _tossFacing);
    }

    private void StopToss()
    {
        if (!_tossing) return;
        _tossing = false;
        _ghostAnimator.SetSpinning(false);
    }
    
    private float _cooldownLeft; // counmt down after the platform expires.

    public void UnlockGhost(Transform from)
    {
        if (_hasGhost) return;
        _hasGhost = true;
        _joining = true;

        Vector2 start = from != null ? (Vector2)from.position : _ghostFollow.ShoulderPosition;
        _ghost.gameObject.SetActive(true);
        spiritTimer1.gameObject.SetActive(true);
        spiritTimer2.gameObject.SetActive(true);
        _ghost.transform.position = start;
        ReturnGhostToGirl(start);
    }

    public void RemoveGhost()
    {
        if (!_hasGhost) return;

        _transition++;
        StopToss();
        if (_ghost.State == CharState.Platform) _ghostPlatform.EndPlatform();
        if (_orb != null) _orb.Cancel();
        GiveControlTo(_girl);

        _hasGhost = false;
        _inTransit = false;
        _joining = false;
        _cooldownLeft = 0f;

        _ghost.Enter(CharState.Following);
        _ghost.gameObject.SetActive(false);

        GhostRemoved?.Invoke();
    }

    public void RecallGhost()
    {
        if (!_hasGhost) return;

        _transition++;
        StopToss();
        bool orbWasFlying = _orb != null && _orb.IsFlying;
        Vector2 from = orbWasFlying ? (Vector2)_orb.transform.position : (Vector2)_ghost.transform.position;
        if (_orb != null) _orb.Cancel();
        _inTransit = false;

        GiveControlTo(_girl);

        if (_ghost.State == CharState.Platform)
        {
            _ghostPlatform.EndPlatform();
            return;
        }
        ReturnGhostToGirl(from);
    }

    private void Awake()
    {
        _ghostPlatform = _ghost.GetComponent<PlatformAbility>();
        _ghostBody = _ghost.GetComponent<CapsuleCollider2D>();
        _ghostController = _ghost.GetComponent<PlatformerController>();
        _girlBody = _girl.GetComponent<CapsuleCollider2D>();
        _ghostAnimator = _ghost.GetComponent<GhostAnimator>();
    }

    private void OnEnable()
    {
        _ghost.StateChanged += OnGhostStateChanged;
        _girlRespawn.Respawned += RecallGhost;
        if (_ghostPlatform != null) _ghostPlatform.Ended += OnPlatformEnded;
    }

    private void OnDisable()
    {
        _ghost.StateChanged -= OnGhostStateChanged;
        _girlRespawn.Respawned -= RecallGhost;
        if (_ghostPlatform != null) _ghostPlatform.Ended -= OnPlatformEnded;
    }


    private void Start()
    {
        _ghost.Enter(CharState.Following);
        GiveControlTo(_girl);
        _ghostUIPassiveSpriteRenderer = _ghostUIPassive.GetComponent<SpriteRenderer>();

        _hasGhost = _startsWithGhost;
        if (!_hasGhost) _ghost.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_hasGhost)
        {
            _girlUIActive.SetActive(true);
            _ghostUIActive.SetActive(false);
            _girlUIPassive.SetActive(false);
            _ghostUIPassive.SetActive(false);
            return;
        }

        if (_active == _girl) {
            _girlUIActive.SetActive(true);
            _ghostUIActive.SetActive(false);
            _girlUIPassive.SetActive(false);
            _ghostUIPassive.SetActive(true);
        }
        else {
            _girlUIActive.SetActive(false);
            _ghostUIActive.SetActive(true);
            _girlUIPassive.SetActive(true);
            _ghostUIPassive.SetActive(false);
        }
        if(_cooldownLeft > 0f || _ghost.State == CharState.Platform) {
            _ghostUIPassiveSpriteRenderer.color = new Color(0.2f,0.2f,0.2f,0.6f);
        }
        else{
            _ghostUIPassiveSpriteRenderer.color = new Color(0.8f,0.8f,0.8f,0.8f);
        }

        if (_cooldownLeft > 0f) _cooldownLeft -= Time.deltaTime;

        if (_tossing) TickToss();

        if (_inTransit) return;

        if (InputBlocker.Blocked) return; // Me and Howe both worked on implementing input blocking. nothing wrong with keeping both implementations.
        if (narrativeUI != null && narrativeUI.narrativeBox.activeSelf) return;
        if (!Input.GetKeyDown(_keys.Swap)) return;

        if (_ghost.State == CharState.Platform) _ghostPlatform.EndPlatform();
        else if (_active == _girl) TryStartDeploy();
        else SwapBackToGirl();

    }

    private void TryStartDeploy()
    {
        if (_cooldownLeft > 0f) return;
        if(_ghost.State == CharState.Platform) return;

        if (!TryGetDeployTarget(out _)) return;

        // PLAY SFX
        AudioController.Instance.ForcePlay(SoundCategory.Ghost, 0);

        _inTransit = true;
        int id = ++_transition;

        _ghostAnimator.PlayDeploy(() =>
        {
            if (id != _transition) return;
            LaunchDeploy();
        });
    }

    private bool TryGetDeployTarget(out Vector2 deployTo)
    {
        Vector2 offset = _deployOffset;
        offset.x *= _girlController.Facing;
        if (!TryFindDeployTarget((Vector2)_girl.transform.position, offset, out deployTo)) return false;
        deployTo = SettleOnFloor(deployTo);
        return true;
    }

    private void LaunchDeploy()
    {
        if (!TryGetDeployTarget(out Vector2 deployTo))
        {
            _inTransit = false;
            return;
        }

        Vector2 from = _ghost.transform.position;
        _girl.Enter(CharState.Idle);

        AudioController.Instance.Mute(SoundCategory.Footsteps);
        AudioController.Instance.Mute(SoundCategory.Land);

        _ghost.Enter(CharState.Travelling);

        if (_cam != null && _orb != null)
            _cam.SetTarget(_orb.transform);

        Fly(from, () => deployTo, _deployTime, () => ArriveDeployed(deployTo));
    }

    private void ArriveDeployed(Vector2 at)
    {
        _inTransit = false;
        _ghostController.Teleport(at);
        GiveControlTo(_ghost);
    }

    private void ReturnGhostToGirl(Vector2 from)
    {
        _ghost.Enter(CharState.Travelling);
        _inTransit = true;

        Fly(from, () => _ghostFollow.ShoulderPosition, _returnTime, ArriveHome);
    }

    private void ArriveHome()
    {
        _ghostFollow.Snap();
        _ghost.Enter(CharState.Following);
        int id = ++_transition;

        _ghostAnimator.PlayReturn(() =>
        {
            if (id != _transition) return;
            _inTransit = false;

            if (_joining)
            {
                _joining = false;
                GhostJoined?.Invoke();
            }
        });
    }

    private void Fly(Vector2 from, Func<Vector2> to, float time, Action onArrive)
    {
        if (_orb == null)
        {
            onArrive();
            return;
        }
        _orb.Fly(from, to, time, onArrive);
    }

    private void OnPlatformEnded()
    {
        _cooldownLeft = _swapCooldown;
        ReturnGhostToGirl(_ghost.transform.position);
    }

    private bool TryFindDeployTarget(Vector2 girlPos, Vector2 offset, out Vector2 target)
    {
        target = girlPos + offset;
        if (_ghostBody == null) return true;

        Vector2 size = Vector2.Scale(_ghostBody.size, _ghost.transform.lossyScale);
        Vector2 colOffset = Vector2.Scale(_ghostBody.offset, _ghost.transform.lossyScale);

        ContactFilter2D filter = GhostFilter();

        float distance = offset.magnitude;
        Vector2 dir = distance > 0.0001f ? offset / distance : Vector2.right;

        if (!TryFindFreeSpot(girlPos, dir, size, colOffset, filter, out Vector2 start)) return false;

        float along = Vector2.Dot(target - start, dir);
        if (along <= 0f)
        {
            target = start;
            return true;
        }

        int count = Physics2D.CapsuleCast(start + colOffset, size, _ghostBody.direction, 0f,
            dir, filter, _deployHits, along);
        if (count == 0)
        {
            target = start + dir * along;
            return true;
        }

        float nearest = along;
        for (int i = 0; i < count; i++) nearest = Mathf.Min(nearest, _deployHits[i].distance);

        float skin = Physics2D.defaultContactOffset * 2f;
        target = start + dir * Mathf.Max(0f, nearest - skin);
        return true;
    }

    private Vector2 SettleOnFloor(Vector2 target)
    {
        if (_ghostBody == null || _girlBody == null) return target;

        Vector2 size = Vector2.Scale(_ghostBody.size, _ghost.transform.lossyScale);
        Vector2 colOffset = Vector2.Scale(_ghostBody.offset, _ghost.transform.lossyScale);

        float ghostBottom = target.y + colOffset.y - size.y * 0.5f;
        float drop = ghostBottom - _girlBody.bounds.min.y;
        if (drop <= 0f) return target;

        int count = Physics2D.CapsuleCast(target + colOffset, size, _ghostBody.direction, 0f,
            Vector2.down, GhostFilter(), _deployHits, drop);
        if (count == 0) return target + Vector2.down * drop;

        float nearest = drop;
        for (int i = 0; i < count; i++) nearest = Mathf.Min(nearest, _deployHits[i].distance);

        float skin = Physics2D.defaultContactOffset * 2f;
        return target + Vector2.down * Mathf.Max(0f, nearest - skin);
    }

    private ContactFilter2D GhostFilter()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(Physics2D.GetLayerCollisionMask(_ghost.gameObject.layer));
        filter.useTriggers = false;
        return filter;
    }

    private bool TryFindFreeSpot(Vector2 from, Vector2 dir, Vector2 size, Vector2 colOffset,
        ContactFilter2D filter, out Vector2 spot)
    {
        float maxShift = Mathf.Max(size.x, size.y);
        for (float d = 0f; d <= maxShift; d += DeployStep)
        {
            if (GhostFits(from + dir * d, size, colOffset, filter))
            {
                spot = from + dir * d;
                return true;
            }
            if (d > 0f && GhostFits(from - dir * d, size, colOffset, filter))
            {
                spot = from - dir * d;
                return true;
            }
        }

        spot = from;
        return false;
    }

    private bool GhostFits(Vector2 position, Vector2 size, Vector2 colOffset, ContactFilter2D filter)
    {
        return Physics2D.OverlapCapsule(position + colOffset, size, _ghostBody.direction, 0f,
            filter, _deployOverlaps) == 0;
    }

    private void SwapBackToGirl()
    {
        GiveControlTo(_girl);
        AudioController.Instance.ForcePlay(SoundCategory.Ghost,1);
        ReturnGhostToGirl(_ghost.transform.position);
    }

    private void GiveControlTo(Character c)
    {
        _active = c;
        _controlChangedFrame = Time.frameCount;
        c.Enter(CharState.Controlled);
        if (_cam != null) _cam.SetTarget(c.transform);

        if (c == _girl)
        {
            AudioController.Instance.Unmute(SoundCategory.Footsteps);
            AudioController.Instance.Unmute(SoundCategory.Land);
        }
    }

    private void OnGhostStateChanged(Character ghost, CharState s)
    {
        // When ghost freezes, pass back to girl to control.
        if (s == CharState.Platform &&  _active == _ghost) GiveControlTo(_girl);

        if (s == CharState.Following || s == CharState.Controlled) _chainCount = 0;
        else if (s == CharState.Platform && _chainCount == 0) _chainCount = 1;
    }
}
    