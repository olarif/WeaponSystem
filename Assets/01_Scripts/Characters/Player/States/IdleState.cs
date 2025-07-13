using UnityEngine;

public class IdleState : MovementState
{
    public IdleState(PlayerController controller) : base(controller) { }

    protected override void CheckTransitions()
    {
        if (!Controller.IsGrounded)
        {
            Controller.StateMachine.ChangeState(new FallingState(Controller));
        }
        else if (Controller.Input.MovementInput.magnitude > 0.1f)
        {
            Controller.StateMachine.ChangeState(new MovingState(Controller));
        }
        else if (Controller.Input.JumpInput && Controller.JumpData.CanJump())
        {
            Controller.StateMachine.ChangeState(new JumpingState(Controller));
        }
    }
}