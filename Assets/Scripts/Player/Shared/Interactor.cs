using UnityEngine;

[RequireComponent(typeof(Character))]
public class Interactor : MonoBehaviour
{
    [SerializeField] private Actor _actor = Actor.Girl;
    [SerializeField] private Keybinds _keys;
    [SerializeField] private float _radius = 0.8f;
    [SerializeField] private LayerMask _interactableLayers;

    public Actor Kind => _actor;

    private Character _character;
    private Interactable _focus;
    private bool _wasBlocked;

    private void Awake() => _character = GetComponent<Character>();

    private void Update()
    {
        bool blocked = InputBlocker.Blocked;
        bool canAct = _character.State == CharState.Controlled && !blocked;

        SetFocus(canAct ? FindBest() : null);

        bool justUnblocked = _wasBlocked && !blocked;
        _wasBlocked = blocked;

        if (!canAct || justUnblocked || _focus == null) return;
        if (Input.GetKeyDown(_keys.Interact)) _focus.Interact(this);
    }

    private Interactable FindBest()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, _radius, _interactableLayers);

        Interactable best = null;
        float bestDist = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            Interactable candidate = hit.GetComponentInParent<Interactable>();
            if (candidate == null) continue;
            if (!candidate.UsableBy(_actor) || !candidate.CanInteract) continue;

            float dist = ((Vector2)hit.transform.position
                        - (Vector2)transform.position).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = candidate;
            }
        }
        return best;
    }

    private void SetFocus(Interactable next)
    {
        if (next == _focus) return;
        if (_focus != null) _focus.SetFocused(false);
        _focus = next;
        if (_focus != null) _focus.SetFocused(true);
    }

    private void OnDisable() => SetFocus(null);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}
