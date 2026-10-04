// SLightly modified the lab 2 animation script. Seems to be working for now, but idk if i'm happy with the implementation.

using System.Collections.Generic;
using UnityEngine;

public class AnimatedEntity : MonoBehaviour
{
    public List<Sprite> DefaultAnimationCycle;
    public float Framerate = 12f;//frames per second
    public SpriteRenderer SpriteRenderer;//spriteRenderer

    private float animationTimer;
    private float animationTimerMax;
    private int index;


    private bool interruptFlag;
    private List<Sprite> interruptAnimation;

    protected virtual void Start()
    {
        AnimationSetup();
    }

    protected virtual void Update()
    {
        AnimationUpdate();
    }

    protected void AnimationSetup()
    {
        if (SpriteRenderer == null) SpriteRenderer = GetComponent<SpriteRenderer>();

        animationTimerMax = 1.0f / Framerate;
        animationTimer = 0;
        index = 0;
    }

    protected void SetCycle(List<Sprite> cycle, float fps)
    {
        if (cycle == DefaultAnimationCycle) return;

        DefaultAnimationCycle = cycle;
        Framerate = fps;
        animationTimerMax = 1.0f / Framerate;
        animationTimer = 0;
        index = 0;
        interruptFlag = false;
        interruptAnimation = null;

        if (cycle != null && cycle.Count > 0)
            SpriteRenderer.sprite = cycle[0];
    }

    protected void SetFlip(bool flip)
    {
        SpriteRenderer.flipX = flip;
    }

    protected void AnimationUpdate()
    {
        animationTimer += Time.deltaTime;

        if (animationTimer > animationTimerMax)
        {
            animationTimer = 0;
            index++;

            if (!interruptFlag)
            {
                if (DefaultAnimationCycle.Count == 0 || index >= DefaultAnimationCycle.Count)
                {
                    index = 0;
                }
                if (DefaultAnimationCycle.Count > 0)
                {
                    SpriteRenderer.sprite = DefaultAnimationCycle[index];
                }
            }
            else
            {
                if (interruptAnimation == null || index >= interruptAnimation.Count)
                {
                    index = 0;
                    interruptFlag = false;
                    interruptAnimation = null;
                }
                else
                {
                    SpriteRenderer.sprite = interruptAnimation[index];
                }
            }
        }
    }

    protected void Interrupt(List<Sprite> _interruptAnimation)
    {
        interruptFlag = true;
        animationTimer = 0;
        index = 0;
        interruptAnimation = _interruptAnimation;
        SpriteRenderer.sprite = interruptAnimation[index];
    }
}
