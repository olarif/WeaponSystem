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
        
        Controller.RotationData.UpdateRotation();
        Controller.HorizontalMovement.UpdateAirMovement();
        
        // Handle air jumping
        if (Controller.Input.JumpPressed && Controller.VerticalMovement.CanJump())
        {
            Controller.VerticalMovement.ExecuteJump();
            Controller.OnJump?.Invoke(true);
        }
        
        Controller.VerticalMovement.ApplyGravity();
        Controller.ApplyMovement();
    }
    
    public override void FixedUpdate()
    {
        
    }
    
    private void CheckTransitions()
    {
        // Air dash
        if (Controller.Input.DashInput && DashState.CanDash())
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