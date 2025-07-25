using UnityEngine;

public class CrouchState : State
{
    public CrouchState(PlayerController controller) : base(controller) { }
    
    public override void Enter()
    {
        //Debug.Log("Started crouching");
    }
    
    public override void Update()
    {
        CheckTransitions();
        
        Controller.MovementData.ApplyMovement();
        Controller.RotationData.UpdateRotation();
        Controller.MovementData.UpdateGroundMovement(PlayerStats.CrouchSpeed);
        
        // Adjust character height
        Controller.MovementData.SetCharacterHeight(PlayerStats.CrouchHeight);
    }
    
    public override void FixedUpdate()
    {
        Controller.MovementData.ApplyGravity();
    }
    
    private void CheckTransitions()
    {
        // Jump while crouching
        if (Controller.Input.JumpInput)
        {
            Controller.StateMachine.ChangeState(new JumpState(Controller));
            return;
        }
        
        // Dash while crouching
        if (Controller.Input.DashInput)
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
    
    public override void Exit()
    {
        Debug.Log("Stopped crouching");
        // Reset character height
        Controller.MovementData.SetCharacterHeight(PlayerStats.StandingHeight);
    }
}