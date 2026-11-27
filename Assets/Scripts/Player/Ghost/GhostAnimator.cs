using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;

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
    [Tooltip("Shown while the cat is in the air and moving up")]
    public List<Sprite> RiseCycle;
    [Tooltip("Shown while the cat is in the air and moving down")]
    public List<Sprite> FallCycle;
    [Tooltip("Played when tossed to the left. Shown exactly as drawn")]
    public List<Sprite> SpinLeftCycle;
    [Tooltip("Played when tossed to the right. Shown exactly as drawn")]
    [FormerlySerializedAs("SpinCycle")]
    public List<Sprite> SpinRightCycle;
    public float SpinFps = 15f;

    private bool _spinning;
    private List<Sprite> _spinCycle;
    private bool _spinFlip;

    public void SetSpinning(bool spinning, int direction = 0)
    {
        _spinning = spinning;
        if (!spinning) return;

        List<Sprite> own = direction < 0 ? SpinLeftCycle : SpinRightCycle;
        List<Sprite> other = direction < 0 ? SpinRightCycle : SpinLeftCycle;

        if (own != null && own.Count > 0)
        {
            _spinCycle = own;
            _spinFlip = false;
        }
        else
        {
            _spinCycle = other;
            _spinFlip = true;
        }
    }

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

    private List<Sprite> AirCycle()
    {
        List<Sprite> cycle = controller.Velocity.y > 0f ? RiseCycle : FallCycle;
        return cycle != null && cycle.Count > 0 ? cycle : null;
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
        List<Sprite> airCycle = controlled && !grounded ? AirCycle() : null;

        if (!IsInterrupting)
        {
            if (_spinning)
            {
                SetCycle(_spinCycle, SpinFps);
            }
            else if (airCycle != null)
            {
                SetCycle(airCycle, JumpFps);
            }
            else if (moving)
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
        if (_spinning) SetFlip(_spinFlip);
        else SetFlip(artFacesLeft ? facing > 0 : facing < 0);

        AnimationUpdate();
    }
}