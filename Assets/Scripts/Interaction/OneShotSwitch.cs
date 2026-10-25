using UnityEngine;

public class OneShotSwitch : Interactable
{
    [Header("One Shot Switch")]
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private Sprite _usedSprite;

    public bool Used { get; private set; }

    public override bool CanInteract => !Used;

    protected override void OnInteract(Interactor who)
    {
        Used = true;
        if (_sprite != null && _usedSprite != null) _sprite.sprite = _usedSprite;
    }
}
