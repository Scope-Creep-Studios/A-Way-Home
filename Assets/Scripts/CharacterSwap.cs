using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSwap : MonoBehaviour
{
    [SerializeField] private Character _a;
    [SerializeField] private Character _b;
    [SerializeField] private KeyCode _swapKey = KeyCode.Tab;
    [SerializeField] private CameraFollow _cam;

    [Header("Respawn B after platform disappears")]
    [SerializeField] private float _respawnDelay = 1f;
    [SerializeField] private Vector2 _respawnOffset = new Vector2(-1.2f, 1.2f);

    private Character _active;

    private void OnEnable() => _b.StateChanged += OnBStateChanged;
    private void OnDisable() => _b.StateChanged -= OnBStateChanged;

    private void Start()
    {
        _b.Enter(CharState.Following);
        GiveControlTo(_a);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(_swapKey)) return;

        if (_active == _a)
        {
            // B cant take over while platform
            bool bBusy = _b.State == CharState.Platform
                      || _b.State == CharState.Despawned;
            if (bBusy) return;
            _a.Enter(CharState.Idle);
            GiveControlTo(_b);
        }
        else
        {
            _b.Enter(CharState.Following);
            GiveControlTo(_a);
        }
    }

    private void GiveControlTo(Character c)
    {
        _active = c;
        c.Enter(CharState.Controlled);
        if (_cam != null) _cam.SetTarget(c.transform);
    }

    private void OnBStateChanged(Character b, CharState s)
    {
        // B just froze into a platform: the player needs a body that can move
        if (s == CharState.Platform && _active == _b) GiveControlTo(_a);

        if (s == CharState.Despawned) StartCoroutine(RespawnB());
    }

    private IEnumerator RespawnB()
    {
        yield return new WaitForSeconds(_respawnDelay);

        _b.transform.position = (Vector2)_a.transform.position + _respawnOffset;
        _b.Enter(CharState.Following);
    }
}
