using UnityEngine;

public class JumpState : State
{
    private static int remainingAirJumps;
    private static bool hasUsedGroundJump = false;
    private static bool isInitialized = false;
    private float jumpBufferTimer;
    
    public JumpState(PlayerController controller) : base(controller) { }
    
    public override void Enter()
    {
        // Initialize jump counts when landing or first time
        if (!isInitialized || (Controller.IsGrounded && !hasUsedGroundJump))
        {
            remainingAirJumps = PlayerStats.MaxAirJumps;
            hasUsedGroundJump = false;
            isInitialized = true;
        }
        
        // Check if we can jump
        if (!CanJump())
        {
            // Can't jump, return to appropriate state
            if (Controller.IsGrounded)
            {
                Controller.StateMachine.ChangeState(new GroundedState(Controller));
            }
            else
            {
                Controller.StateMachine.ChangeState(new AirState(Controller));
            }
            return;
        }
        
        ExecuteJump();
        Controller.OnJump?.Invoke(true);
        
        
        if (Controller.IsGrounded)
        {
            //Normal grounded jump
            //Debug.Log("Ground jump executed!");
        }
        else
        {
            //air jump
            //Debug.Log($"Air jump executed! Remaining air jumps: {remainingAirJumps}");
        }
    }
    
    private bool CanJump()
    {
        if (Controller.IsGrounded)
        {
            // Can always ground jump if not used it yet
            return !hasUsedGroundJump;
        }
        else
        {
            // Can air jump only if we have air jumps remaining
            return remainingAirJumps > 0;
        }
    }
    
    private void ExecuteJump()
    {
        float jumpVelocity = Mathf.Sqrt(PlayerStats.JumpForce * -2f * PlayerStats.Gravity);
        Controller.MovementData.SetYVelocity(jumpVelocity);
        
        if (Controller.IsGrounded)
        {
            // Use ground jump
            hasUsedGroundJump = true;
        }
        else
        {
            // Use air jump
            remainingAirJumps--;
        }
        
        jumpBufferTimer = 0f;
    }
    
    public override void Update()
    {
        // Update jump buffer for ground jumps
        if (Controller.Input.JumpInput)
        {
            jumpBufferTimer = PlayerStats.JumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }
        
        CheckTransitions();
        
        Controller.MovementData.ApplyMovement();
        Controller.RotationData.UpdateRotation();
        Controller.MovementData.UpdateAirMovement();
        Controller.MovementData.ApplyGravity();
    }
    
    public override void FixedUpdate()
    {
        
    }
    
    private void CheckTransitions()
    {
        // air jumps
        if (Controller.Input.JumpInput && CanJump())
        {
            ExecuteJump();
            return;
        }
        
        // Dash
        if (Controller.Input.DashInput)
        {
            Controller.StateMachine.ChangeState(new DashState(Controller));
            return;
        }
        
        // Transition to falling when upward velocity stops
        if (Controller.MovementData.YVelocity <= 0)
        {
            Controller.StateMachine.ChangeState(new AirState(Controller));
            return;
        }
        
        // Land during jump
        if (Controller.IsGrounded)
        {
            ResetJumpsOnLanding();
            Controller.StateMachine.ChangeState(new GroundedState(Controller));
            return;
        }
    }

    public override void Exit()
    {
        Controller.OnJump?.Invoke(false);
    }
    
    // Static methods for external reset
    public static void ResetJumps()
    {
        hasUsedGroundJump = false;
        remainingAirJumps = 0;
        isInitialized = false;
    }
    
    public static void ResetJumpsOnLanding()
    {
        hasUsedGroundJump = false;
        // remainingAirJumps will be set when next jump state is entered
    }
    
    // Debug info
    public static int GetRemainingAirJumps() => remainingAirJumps;
    public static bool HasUsedGroundJump() => hasUsedGroundJump;
}