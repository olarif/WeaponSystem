using UnityEngine;

public class GroundedState : State
{
    private bool _wasSprintingLastFrame = false;
    
    public GroundedState(PlayerController controller) : base(controller) { }
    
    public override void Enter()
    {
        //Debug.Log("Entered Grounded State");
        UpdateSprintEvents();
    }
    
    public override void Update()
    {
        UpdateSprintEvents();
        CheckTransitions();

        float targetSpeed = GetTargetSpeed();
        Controller.MovementData.UpdateGroundMovement(targetSpeed);
        
        Controller.RotationData.UpdateRotation();
        Controller.MovementData.ApplyMovement();
        Controller.MovementData.ApplyGravity();
    }
    
    public override void FixedUpdate()
    {

    }
    
    private float GetTargetSpeed()
    {
        return Controller.Input.SprintInput ? PlayerStats.SprintSpeed : PlayerStats.WalkSpeed;
    }
    
    private void UpdateSprintEvents()
    {
        bool isSprintingNow = Controller.Input.SprintInput;
        
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
    
    private void CheckTransitions()
    {
        // Jump takes priority
        if (Controller.Input.JumpInput)
        {
            Controller.StateMachine.ChangeState(new JumpState(Controller));
            return;
        }
        
        // Dash
        if (Controller.Input.DashInput)
        {
            Controller.StateMachine.ChangeState(new DashState(Controller));
            return;
        }
        
        // Crouch
        if (Controller.Input.CrouchInput)
        {
            Controller.StateMachine.ChangeState(new CrouchState(Controller));
            return;
        }
        
        // Fall off edge
        if (!Controller.IsGrounded)
        {
            Controller.StateMachine.ChangeState(new AirState(Controller));
            return;
        }
    }
    
    public override void Exit()
    {
        if (_wasSprintingLastFrame)
        {
            Controller.OnSprint?.Invoke(false);
        }
    }
}