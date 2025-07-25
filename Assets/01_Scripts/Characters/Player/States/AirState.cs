using UnityEngine;

public class AirState : State
{
    public AirState(PlayerController controller) : base(controller) { }
    
    public override void Enter()
    {
        //Debug.Log("Entered Air State");
    }
    
    public override void Update()
    {
        CheckTransitions();
        
        Controller.MovementData.ApplyMovement();
        Controller.MovementData.UpdateAirMovement();
        Controller.RotationData.UpdateRotation();
        Controller.MovementData.ApplyGravity();
    }
    
    public override void FixedUpdate()
    {
        
    }
    
    private void CheckTransitions()
    {
        // Jump (multi-jump)
        if (Controller.Input.JumpInput)
        {
            Controller.StateMachine.ChangeState(new JumpState(Controller));
            return;
        }
        
        // Air dash
        if (Controller.Input.DashInput)
        {
            Controller.StateMachine.ChangeState(new DashState(Controller));
            return;
        }
        
        // Land
        if (Controller.IsGrounded)
        {
            Controller.StateMachine.ChangeState(new GroundedState(Controller));
            return;
        }
    }
}