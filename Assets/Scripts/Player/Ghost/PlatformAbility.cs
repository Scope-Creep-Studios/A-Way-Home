using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class PlatformAbility : MonoBehaviour
{
    [SerializeField] private Keybinds _keys;
    [SerializeField] private float _lifetime = 3f;
    [Tooltip("Add the girl's layer here so she cant be trapped inside the platform.")]
    [SerializeField] private LayerMask _blockedby;
    [SerializeField] private GameObject _spiritPlatformUI;
    [SerializeField] private float _nudgeStep = 0.05f;

    private Character _character;
    private PlatformerController _controller;
    private float _timeLeft;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _controller = GetComponent<PlatformerController>();
    }

    private void Update()
    {
        if (_character.State == CharState.Controlled)
        {
            if (InputBlocker.Blocked) return;
            bool pressed = Input.GetKeyDown(_keys.BecomePlatform);
            if (pressed && TryFindRoom(out Vector2 spot))
            {
                if (spot != (Vector2)transform.position) _controller.Teleport(spot);
                _timeLeft = _lifetime;
                _character.Enter(CharState.Platform);
            }
        }
        else if (_character.State == CharState.Platform)
        {
            _spiritPlatformUI.transform.localScale = new Vector3(_timeLeft * 2,0.3f,1f);
            _spiritPlatformUI.transform.localPosition = new Vector3(_timeLeft - 6.42f, 7.4f, 10f);
            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f) {
                _character.Enter(CharState.Following);
                _spiritPlatformUI.transform.localScale = new Vector3(_lifetime * 2,0.3f,1f);
                _spiritPlatformUI.transform.localPosition = new Vector3(_timeLeft - 3.42f, 7.4f, 10f);
            }
        }
    }

    private bool TryFindRoom(out Vector2 spot)
    {
        BoxCollider2D box = _character.PlatformCollider;
        Vector2 size = Vector2.Scale(box.size, transform.lossyScale);
        Vector2 offset = Vector2.Scale(box.offset, transform.lossyScale);
        Vector2 origin = transform.position;
        float maxNudge = size.x * 0.5f;
        float step = Mathf.Max(_nudgeStep, 0.01f);

        for (float d = 0f; d <= maxNudge; d += step)
        {
            if (Fits(origin + Vector2.right * d, offset, size))
            {
                spot = origin + Vector2.right * d;
                return true;
            }
            if (d > 0f && Fits(origin + Vector2.left * d, offset, size))
            {
                spot = origin + Vector2.left * d;
                return true;
            }
        }

        spot = origin;
        return false;
    }

    private bool Fits(Vector2 position, Vector2 offset, Vector2 size)
    {
        return Physics2D.OverlapBox(position + offset, size, 0f, _blockedby) == null;
    }

    private void OnDrawGizmosSelected()
    {
        if (_character == null) _character = GetComponent<Character>();
        BoxCollider2D box = _character.PlatformCollider;
        if (box == null) return;

        Vector2 size = Vector2.Scale(box.size, transform.lossyScale);
        Vector2 offset = Vector2.Scale(box.offset, transform.lossyScale);
        Vector2 here = transform.position;

        bool fitsHere = Fits(here, offset, size);
        Gizmos.color = fitsHere ? Color.green : Color.red;
        Gizmos.DrawWireCube(here + offset, size);

        if (!fitsHere && TryFindRoom(out Vector2 spot))
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(spot + offset, size);
        }
    }
}
