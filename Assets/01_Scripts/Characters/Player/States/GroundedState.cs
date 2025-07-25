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
        CheckTransitions();

        Controller.RotationData.UpdateRotation();
        
        float targetSpeed = GetTargetSpeed();
        Controller.HorizontalMovement.UpdateGroundMovement(targetSpeed);
        
        // Handle sprinting stamina consumption
        if (Controller.Input.SprintInput && Controller.StaminaComponent.CanSprint)
        {
            Controller.StaminaComponent.TryUse(PlayerStats.SprintStaminaCost * Time.deltaTime);
            UpdateSprintEvents();
        }

        if ((Controller.Input.JumpPressed || Controller.Input.JumpHeld) && Controller.VerticalMovement.CanJump())
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
    
    private float GetTargetSpeed()
    {
        if (Controller.Input.CrouchInput)
            return PlayerStats.CrouchSpeed;
        else if (Controller.Input.SprintInput && Controller.StaminaComponent.CanSprint)
            return PlayerStats.SprintSpeed;
        else
            return PlayerStats.WalkSpeed;
    }
    
    private void UpdateSprintEvents()
    {
        bool isSprintingNow = Controller.Input.SprintInput;
        
        if (isSprintingNow && !_wasSprintingLastFrame && Controller.StaminaComponent.CanSprint)
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
        // Dash
        if (Controller.Input.DashInput && DashState.CanDash())
        {
            Controller.StateMachine.ChangeState(new DashState(Controller));
            return;
        }
        
        // Crouch (only if we can fit)
        if (Controller.Input.CrouchInput && !IsAlreadyCrouching())
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
    
    private bool IsAlreadyCrouching()
    {
        // Check if we're already at crouch height
        CharacterController cc = Controller.GetComponent<CharacterController>();
        return Mathf.Approximately(cc.height, PlayerStats.CrouchHeight);
    }
    
    public override void Exit()
    {
        if (_wasSprintingLastFrame)
        {
            Controller.OnSprint?.Invoke(false);
        }
    }
}