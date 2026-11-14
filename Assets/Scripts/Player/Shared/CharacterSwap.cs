using System;
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

    [Header("Spirit orb")]
    [SerializeField] private SpiritOrb _orb;
    [SerializeField] private float _returnTime = 0.25f;

    [Header("Cooldown after platform expiry")]
    [SerializeField] private float _swapCooldown = 1f;

    [Header("UI Objects")]
    [SerializeField] private GameObject _girlUIActive;
    [SerializeField] private GameObject _ghostUIActive;
    [SerializeField] private GameObject _girlUIPassive;
    [SerializeField] private GameObject _ghostUIPassive;


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
    
    private float _cooldownLeft; // counmt down after the platform expires.

    public void RecallGhost()
    {
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

    }

    private void Update()
    {
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

        Vector2 offset = _deployOffset;
        offset.x *= _girlController.Facing;
        if (!TryFindDeployTarget((Vector2)_girl.transform.position, offset, out Vector2 deployTo)) return;
        deployTo = SettleOnFloor(deployTo);

        Vector2 from = _ghost.transform.position;
        _girl.Enter(CharState.Idle);

        // PLAY SFX
        AudioController.Instance.ForcePlay(SoundCategory.Ghost, 0);

        AudioController.Instance.Mute(SoundCategory.Footsteps);
        AudioController.Instance.Mute(SoundCategory.Land);

        _inTransit = true;

        _ghostAnimator.PlayDeploy(() =>
        {
            _ghost.Enter(CharState.Travelling);

            if (_cam != null && _orb != null)
                _cam.SetTarget(_orb.transform);

            Fly(from, () => deployTo, _deployTime, () => ArriveDeployed(deployTo));
        });
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
        _ghost.Enter(CharState.Deploying);

        _ghostAnimator.PlayReturn(() =>
        {
            _ghost.Enter(CharState.Following);
            _ghostFollow.Snap();
            _inTransit = false;
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
    }
}
    