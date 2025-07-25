using UnityEngine;

public class CrouchState : State
{
    public CrouchState(PlayerController controller) : base(controller) { }
    
    public override void Enter()
    {
        //Debug.Log("Started crouching");
        SetCharacterHeight(PlayerStats.CrouchHeight);
    }
    
    public override void Update()
    {
        CheckTransitions();
        
        Controller.RotationData.UpdateRotation();
        Controller.HorizontalMovement.UpdateGroundMovement(PlayerStats.CrouchSpeed);
        
        // Handle jumping while crouchedHeldHeldInPressedGHeldPreHPressed_controller.Input.JumpRessPressed || _controller.Input.JumpHeld_controller.Input.
        if (Controller.Input.JumpPressed && Controller.VerticalMovement.CanJump())
        {
            Controller.VerticalMovement.ExecuteJump();
            Controller.OnJump?.Invoke(true);
            // Jump out of crouch
            Controller.StateMachine.ChangeState(new GroundedState(Controller));
            return;
        }
        
        Controller.VerticalMovement.ApplyGravity();
        Controller.ApplyMovement();
    }
    
    public override void FixedUpdate()
    {

    }
    
    private void CheckTransitions()
    {
        // Dash while crouching
        if (Controller.Input.DashInput && DashState.CanDash())
        {
            Controller.StateMachine.ChangeState(new DashState(Controller));
            return;
        }
        
        // Stop crouching
        if (!Controller.Input.CrouchInput)
        {
            if (CanStandUp())
            {
                Controller.StateMachine.ChangeState(new GroundedState(Controller));
            }
            // If can't stand up, stay crouched
            return;
        }
        
        // Fall while crouching
        if (!Controller.IsGrounded)
        {
            Controller.StateMachine.ChangeState(new AirState(Controller));
            return;
        }
    }
    
    private bool CanStandUp()
    {
        Vector3 bottom = Controller.transform.position;
        Vector3 top = bottom + Vector3.up * PlayerStats.StandingHeight;
        float radius = Controller.GetComponent<CharacterController>().radius;
        
        return !Physics.CheckCapsule(bottom, top, radius, PlayerStats.GroundLayer);
    }
    
    private void SetCharacterHeight(float height)
    {
        CharacterController cc = Controller.GetComponent<CharacterController>();
        Vector3 center = cc.center;
        
        // Adjust center to keep feet on ground
        center.y = height / 2f;
        cc.height = height;
        cc.center = center;
    }
    
    public override void Exit()
    {
        //Debug.Log("Stopped crouching");
        SetCharacterHeight(PlayerStats.StandingHeight);
    }
}