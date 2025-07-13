using UnityEngine;

public abstract class MovementState : State
{
    protected MovementState(PlayerController controller) : base(controller) { }

    protected virtual void HandleMovement()
    {
        Controller.MovementData.UpdateMovement();
    }
    
    protected virtual void HandleRotation()
    {
        Controller.RotationData.UpdateRotation();
    }

    public override void Update()
    {
        HandleMovement();
        HandleRotation();
        CheckTransitions();
    }

    protected abstract void CheckTransitions();
}