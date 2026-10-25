using UnityEngine;
using UnityEngine.Events;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interactable")]
    [SerializeField] private Actor _usableBy = Actor.Both;
    [SerializeField] private GameObject _prompt;

    public UnityEvent Interacted;

    public virtual bool CanInteract => true;

    public bool UsableBy(Actor actor) => (_usableBy & actor) != 0;

    protected virtual void Awake()
    {
        if (_prompt != null) _prompt.SetActive(false);
    }

    public void SetFocused(bool focused)
    {
        if (_prompt != null) _prompt.SetActive(focused);
    }

    public void Interact(Interactor who)
    {
        if (!CanInteract) return;
        OnInteract(who);
        Interacted?.Invoke();
    }

    protected abstract void OnInteract(Interactor who);
}
