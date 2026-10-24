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

    private Character _character;
    private float _timeLeft;

    private void Awake() => _character = GetComponent<Character>();

    private void Update()
    {
        if (_character.State == CharState.Controlled)
        {
            if (InputBlocker.Blocked) return;
            bool pressed = Input.GetKeyDown(_keys.BecomePlatform);
            if (pressed && !SomethingInTheWay())
            {
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

    private bool SomethingInTheWay()
    {
        BoxCollider2D box = _character.PlatformCollider;
        Vector2 centre = (Vector2)transform.position + box.offset;
        return Physics2D.OverlapBox(centre, box.size, 0f, _blockedby) != null;
    }
}
