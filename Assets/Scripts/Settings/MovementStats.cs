using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MovementStats", menuName = "Platformer/Movement Stats")]
public class MovementStats : ScriptableObject
{
    // Layers to collide with
    [Header("Collision Layers")]
    [Tooltip("The layers the character can collide with")]
    public LayerMask GroundLayers;

    // [Header("Input Standardization")]
    // [Tooltip("Standardize the input intensity so M&K and Controller has the same feeling.")]

    // Stats related to horizontal movement like running, etc...
    [Header("Running")]
    [Tooltip("Top horizontal speed in units/s")]
    public float MaxSpeed = 14f;
    [Tooltip("How fast we gain speed, units/s/s")]
    public float Acceleration = 120f;
    [Tooltip("How fast we stop on the ground with no input")]
    public float GroundDeceleration = 60f;
    [Tooltip("How fast we stop in the air with no input")]
    public float AirDeceleration = 30;

    [Header("Grounding and floor detection")]
    [Tooltip("Small constant downward force, solution to slopes")]
    public float GroundingForce = -1.5f;
    [Tooltip("How far to check above and below the character for things like ground/ceiling")]
    [Range(0f, 0.5f)] public float GroundColDistance = 0.05f;

    [Header("Jumps and Gravity")]
    [Tooltip("Upward speed set when we jump")]
    public float JumpPower = 36f;
    [Tooltip("Terminal Velocity; Maximum falling speed")]
    public float MaxFallSpeed = 40f;
    [Tooltip("Our gravity, in units per second, per second")]
    public float FallAcceleration = 110f;
    [Tooltip("Gravity is multiplied by this while rising after letting go of jump")]
    public float EndJumpEarlyGravityModifier = 3f;
    [Tooltip("Gravity is multiplied by this while falling. Above 1 makes jumps come down faster than they go up")]
    public float FallGravityMultiplier = 1f;
    [Tooltip("How long after walking off a ledge a jump still works")]
    public float CoyoteTime = 0.15f;
    [Tooltip("How long before landing a jump press is remembered")]
    public float JumpBuffer = 0.2f;

    [Header("Feel: turning")]
    [Tooltip("1 to trun it off")]
    public float TurnAccelerationMultiplier = 1.8f;

    [Header("Feel: assists")]
    [Tooltip("Nudge up onto a ledge we barely miss")]
    public bool LedgeAssist = true;
    public float LedgeAssistHeight = 0.3f;
    [Tooltip("how fast we can be moving and have ledge assist happen")]
    public float LedgeAssistMaxRiseSpeed = 4f;
    [Tooltip("Slide around a ceiling corner we hit our head on")]
    public bool CornerCorrection = true;
    public float CornerCorrectionWidth = 0.3f;
    [Tooltip("Stay on grounded during tiny drops")]
    public bool GroundSnap = true;
    public float GroundSnapDistance = 0.3f;
}
