using UnityEngine;

public class JumpingState : MovementState
{
    public JumpingState(PlayerController controller) : base(controller) { }
    
    public override void Enter()
    {
        Controller.JumpData.ExecuteJump();
        Controller.OnJump?.Invoke(true);
    }
    
    public override void Exit()
    {
        Controller.OnJump?.Invoke(false);
    }
    
    protected override void CheckTransitions()
    {
        if (Controller.MovementData.YVelocity <= 0)
        {
            Controller.StateMachine.ChangeState(new FallingState(Controller));
        }
    }
}