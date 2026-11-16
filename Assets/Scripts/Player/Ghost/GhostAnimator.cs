using System.Collections.Generic;
using UnityEngine;
using System;

public class GhostAnimator : AnimatedEntity
{
    [SerializeField] private Character character;
    [SerializeField] private PlatformerController controller;      
    [SerializeField] private PlatformerController girlController;   
    [Tooltip("Tick this if the art faces LEFT in the source images")]
    [SerializeField] private bool artFacesLeft = true;

    [Header("Cycles")]
    public List<Sprite> ShoulderCycle;   
    public float ShoulderFps = 8f;
    public List<Sprite> ActiveCycle; 
    public float ActiveFps = 10f;
    public List<Sprite> DeployCycle;
    public float DeployFps = 15f;
    public List<Sprite> ReturnCycle;
    public float ReturnFps = 15f;
    public List<Sprite> JumpCycle;
    public float JumpFps = 8f;

    private CharState previousState;
    private bool wasGrounded;

    public void PlayDeploy(System.Action onComplete)
    {
        Interrupt(DeployCycle, DeployFps, onComplete);
    }

    public void PlayReturn(System.Action onComplete)
    {
        Interrupt(ReturnCycle, ReturnFps, onComplete);
    }

    protected override void Start()
    {
        base.Start();
        previousState = character.State;
        wasGrounded = controller.Grounded;
    }

    protected override void Update()
    {
        if (character.State != previousState)
        {
            previousState = character.State;
        }

        bool controlled = character.State == CharState.Controlled;
        bool grounded = controller.Grounded;

        if (controlled && wasGrounded && !grounded && controller.Velocity.y > 0f)
        {
            if (JumpCycle != null && JumpCycle.Count > 0)
                Interrupt(JumpCycle, JumpFps);
        }

        if (controlled)
        {
            wasGrounded = grounded;
        }
        bool moving = controlled && controller.MoveX != 0f;

        if (!IsInterrupting)
        {
            if (moving)
            {
                SetCycle(ActiveCycle, ActiveFps);
                if (index == 0 || index == 4)
                {
                    AudioController.Instance.PlayRandom(SoundCategory.GhostSteps);
                }
            }
            else SetCycle(ShoulderCycle, ShoulderFps);
        }
        int facing = controlled ? controller.Facing : girlController.Facing;
        SetFlip(artFacesLeft ? facing > 0 : facing < 0);

        AnimationUpdate();
    }
}