using System.Collections.Generic;
using UnityEngine;

public class GirlAnimator : AnimatedEntity
{
    [SerializeField] private PlatformerController controller;
    [Tooltip("Tick this if the art faces LEFT in the source images")]
    [SerializeField] private bool artFacesLeft = true;

    [Header("Cycles")]
    public List<Sprite> IdleCycle;
    public float IdleFps = 8f;
    public List<Sprite> RunCycle;
    public float RunFps = 12f;
    public List<Sprite> AirborneCycle;
    public float AirborneFps = 8f;
    public List<Sprite> JumpyJump;

    [SerializeField] private float airborneDelay = 0.08f;

    private bool wasGrounded;
    private float _airTime;

    protected override void Start()
    {
        base.Start();
        wasGrounded = controller.Grounded;
    }

    protected override void Update()
    {
        bool grounded = controller.Grounded;

        if (grounded) _airTime = 0f;
        else _airTime += Time.deltaTime;
        bool looksGrounded = grounded || (_airTime < airborneDelay && controller.Velocity.y <= 0f);

        if (wasGrounded && !grounded && controller.Velocity.y > 0f)
        {
            SetCycle(AirborneCycle, AirborneFps);
            if (JumpyJump != null && JumpyJump.Count > 0) Interrupt(JumpyJump);
        }
        else if (looksGrounded)
        {
            if (controller.MoveX != 0f)
            { 
                SetCycle(RunCycle, RunFps);
                if (index == 1 || index == 4)
                {
                    AudioController.Instance.PlayRandom(SoundCategory.Footsteps);
                }
            }
            else{ SetCycle(IdleCycle, IdleFps);}
        }
        else
        {
            SetCycle(AirborneCycle, AirborneFps); 
        }
        wasGrounded = grounded;

        bool facingLeft = controller.Facing < 0;
        SetFlip(artFacesLeft ? !facingLeft : facingLeft);

        AnimationUpdate();
    }
}
