using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenPlatform : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;

    public bool IsPermanent { get; private set; }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();

        IsPermanent = false;
        SetVisible(false);
    }

    // temporarily show or hide the platform through CatVision.
    public void SetVisible(bool visible)
    {
        // Permanent platforms should never be hidden again.
        if (IsPermanent)
            return;

        if (spriteRenderer != null)
            spriteRenderer.enabled = visible;

        if (platformCollider != null)
            platformCollider.enabled = visible;
    }

    // permanently reveal the platform after activating the light
    public void MakePermanent()
    {
        IsPermanent = true;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        if (platformCollider != null)
            platformCollider.enabled = true;
    }
}