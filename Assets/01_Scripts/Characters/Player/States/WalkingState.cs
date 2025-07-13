using UnityEngine;

public class MovingState : MovementState
{
    public MovingState(PlayerController controller) : base(controller) { }
    
    public override void Enter()
    {
        CheckSprintState();
    }
    
    public override void Update()
    {
        base.Update();
        CheckSprintState(); // Check every frame for sprint events
    }
    
    private bool _wasSprintingLastFrame = false;
    
    private void CheckSprintState()
    {
        bool isSprintingNow = Controller.MovementData.IsSprinting;
        
        if (isSprintingNow && !_wasSprintingLastFrame)
        {
            Controller.OnSprint?.Invoke(true);
        }
        else if (!isSprintingNow && _wasSprintingLastFrame)
        {
            Controller.OnSprint?.Invoke(false);
        }
        
        _wasSprintingLastFrame = isSprintingNow;
    }

    protected override void CheckTransitions()
    {
        if (!Controller.IsGrounded)
        {
            Controller.StateMachine.ChangeState(new FallingState(Controller));
        }
        else if (Controller.Input.MovementInput.magnitude <= 0.1f && !Controller.IsMoving)
        {
            Controller.StateMachine.ChangeState(new IdleState(Controller));
        }
        else if (Controller.Input.JumpInput && Controller.JumpData.CanJump())
        {
            Controller.StateMachine.ChangeState(new JumpingState(Controller));
        }
    }
    
    public override void Exit()
    {
        // Make sure to turn off sprint event when leaving moving state
        if (_wasSprintingLastFrame)
        {
            Controller.OnSprint?.Invoke(false);
        }
    }
}