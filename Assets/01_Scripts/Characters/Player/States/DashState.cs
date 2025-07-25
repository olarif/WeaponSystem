using UnityEngine;

public class DashState : State
{
    private static float dashCooldown = 0f;
    private float dashTimer;
    private Vector3 dashDirection;
    
    public DashState(PlayerController controller) : base(controller) { }
    
    public override void Enter()
    {
        // Check cooldown
        if (dashCooldown > 0f)
        {
            ReturnToAppropriateState();
            return;
        }
        
        // Check stamina if required
        if (!PlayerStats.AllowDashWithoutStamina && !Controller.StaminaComponent.CanAfford(PlayerStats.DashStaminaCost))
        {
            ReturnToAppropriateState();
            return;
        }
    
        // Use stamina if required
        if (!PlayerStats.AllowDashWithoutStamina)
        {
            Controller.StaminaComponent.TryUse(PlayerStats.DashStaminaCost);
        }
        
        dashDirection = GetDashDirection();
        Vector3 dashForce = dashDirection * PlayerStats.DashForce;
        
        // Add upward force for air dashes
        if (!Controller.IsGrounded && PlayerStats.AirDashUpwardForce > 0f)
        {
            dashForce.y = PlayerStats.AirDashUpwardForce;
        }
        
        // Apply the force (handled by HorizontalMovementData and VerticalMovementData)
        Controller.ApplyForce(dashForce);
        
        // Set timers
        dashTimer = PlayerStats.DashDuration;
        dashCooldown = PlayerStats.DashCooldown;
        
        //Debug.Log("Dash started!");
    }
    
    public override void Update()
    {
        dashTimer -= Time.deltaTime;
        
        CheckTransitions();
        
        // Handle rotation during dash
        Controller.RotationData.UpdateRotation();
        
        // Don't update horizontal movement during dash
        
        Controller.VerticalMovement.ApplyGravity();
        Controller.ApplyMovement();
    }
    
    public override void FixedUpdate()
    {
        
    }
    
    private Vector3 GetDashDirection()
    {
        Vector2 inputMovement = Controller.Input.MovementInput;
        
        if (inputMovement.magnitude > 0.1f)
        {
            // Dash in input direction
            Vector3 cameraForward = GetCameraForward();
            Vector3 cameraRight = GetCameraRight();
            return (cameraForward * inputMovement.y + cameraRight * inputMovement.x).normalized;
        }
        else
        {
            // No input - dash forward
            return Controller.transform.forward;
        }
    }
    
    private Vector3 GetCameraForward()
    {
        Vector3 forward = Controller.PlayerCamera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }
    
    private Vector3 GetCameraRight()
    {
        Vector3 right = Controller.PlayerCamera.transform.right;
        right.y = 0;
        return right.normalized;
    }
    
    private void CheckTransitions()
    {
        // Dash duration finished
        if (dashTimer <= 0f)
        {
            ReturnToAppropriateState();
        }
    }
    
    private void ReturnToAppropriateState()
    {
        if (Controller.IsGrounded)
        {
            Controller.StateMachine.ChangeState(new GroundedState(Controller));
        }
        else
        {
            Controller.StateMachine.ChangeState(new AirState(Controller));
        }
    }
    
    public override void Exit()
    {
        Debug.Log("Dash ended!");
    }
    
    // Static method to update cooldown
    public static void UpdateCooldown()
    {
        if (dashCooldown > 0f)
        {
            dashCooldown -= Time.deltaTime;
        }
    }
    
    public static bool CanDash()
    {
        return dashCooldown <= 0f;
    }
    
    public static void ResetDashOnLand()
    {
        dashCooldown = 0f;
    }
}