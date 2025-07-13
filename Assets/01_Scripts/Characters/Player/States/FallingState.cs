using UnityEngine;

public class FallingState : MovementState
{
    public FallingState(PlayerController controller) : base(controller) { }
    
    protected override void CheckTransitions()
    {
        if (Controller.IsGrounded)
        {
            if (Controller.Input.MovementInput.magnitude > 0.1f)
            {
                Controller.StateMachine.ChangeState(new MovingState(Controller));
            }
            else
            {
                Controller.StateMachine.ChangeState(new IdleState(Controller));
            }
        }
    }
}