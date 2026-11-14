// SLightly modified the lab 2 animation script. Seems to be working for now, but idk if i'm happy with the implementation.
using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedEntity : MonoBehaviour
{
    public List<Sprite> DefaultAnimationCycle;
    public float Framerate = 12f;//frames per second
    public SpriteRenderer SpriteRenderer;//spriteRenderer

    private float animationTimer;
    private float animationTimerMax;
    public int index;


    private bool interruptFlag;
    protected bool IsInterrupting => interruptFlag;
    private List<Sprite> interruptAnimation;
    private Action interruptCompleted;

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
        interruptCompleted = null;

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
            animationTimer -= animationTimerMax;
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
                    Action callback = interruptCompleted;
                    interruptCompleted = null;
                    callback?.Invoke();
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
        Interrupt(_interruptAnimation, Framerate);
    }
    protected void Interrupt(List<Sprite> _interruptAnimation, float fps)
    {
        interruptFlag = true;
        animationTimer = 0;
        index = 0;
        interruptAnimation = _interruptAnimation;
        animationTimerMax = 1.0f / fps;
        SpriteRenderer.sprite = interruptAnimation[index];
    }

    protected void Interrupt(List<Sprite> _interruptAnimation,float fps,Action onComplete)
    {
        interruptCompleted = onComplete;
        Interrupt(_interruptAnimation, fps);
    }
}