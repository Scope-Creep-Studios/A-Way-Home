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

    private Character _character;
    private float _timeLeft;

    private void Awake() => _character = GetComponent<Character>();

    private void Update()
    {
        if (_character.State == CharState.Controlled)
        {
            bool pressed = Input.GetKeyDown(_keys.BecomePlatform);
            if (pressed && !SomethingInTheWay())
            {
                _timeLeft = _lifetime;
                _character.Enter(CharState.Platform);
            }
        }
        else if (_character.State == CharState.Platform)
        {
            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f) _character.Enter(CharState.Following);
        }
    }

    private bool SomethingInTheWay()
    {
        BoxCollider2D box = _character.PlatformCollider;
        Vector2 centre = (Vector2)transform.position + box.offset;
        return Physics2D.OverlapBox(centre, box.size, 0f, _blockedby) != null;
    }
}
