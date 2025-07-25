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
            // Can't dash, return to appropriate state
            ReturnToAppropriateState();
            return;
        }
        
        // Calculate dash direction
        dashDirection = GetDashDirection();
        
        // Apply dash velocity
        Controller.MovementData.SetVelocity(dashDirection * PlayerStats.DashForce);
        
        // Add upward component for air dashes
        if (!Controller.IsGrounded && PlayerStats.AirDashUpwardForce > 0f)
        {
            Controller.MovementData.SetYVelocity(
                Mathf.Max(Controller.MovementData.YVelocity, PlayerStats.AirDashUpwardForce)
            );
        }
        
        // Set timers
        dashTimer = PlayerStats.DashDuration;
        dashCooldown = PlayerStats.DashCooldown;
        
        //Debug.Log("Dash started!");
    }
    
    public override void Update()
    {
        // Update dash timer
        dashTimer -= Time.deltaTime;
        
        // Don't allow other inputs during dash
        CheckTransitions();
        
        Controller.MovementData.ApplyMovement();
        Controller.RotationData.UpdateRotation();
    }
    
    public override void FixedUpdate()
    {
        Controller.MovementData.ApplyGravity();
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