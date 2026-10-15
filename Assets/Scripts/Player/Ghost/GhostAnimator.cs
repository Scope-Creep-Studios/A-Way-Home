using System.Collections.Generic;
using UnityEngine;

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

    protected override void Update()
    {
        bool controlled = character.State == CharState.Controlled;

        if (controlled) SetCycle(ActiveCycle, ActiveFps);
        else SetCycle(ShoulderCycle, ShoulderFps);

        int facing = controlled ? controller.Facing : girlController.Facing;
        SetFlip(artFacesLeft ? facing > 0 : facing < 0);

        AnimationUpdate();
    }
}
