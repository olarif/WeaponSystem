using UnityEngine;

[System.Serializable]
public class MovementData
{
    private PlayerController _controller;
    private CharacterController _characterController;
    private PlayerStatsSO _stats;
    
    [Header("Movement State")]
    [SerializeField] private Vector3 velocity;
    [SerializeField] private float yVelocity;
    
    // Properties
    public Vector3 Velocity => velocity;
    public float YVelocity => yVelocity;
    public bool IsMoving => velocity.magnitude > 0.1f;
    public float CurrentSpeed => velocity.magnitude;
    
    public void Initialize(PlayerController controller, CharacterController characterController, PlayerStatsSO stats)
    {
        _controller = controller;
        _characterController = characterController;
        _stats = stats;
        
        velocity = Vector3.zero;
        yVelocity = 0f;
    }

    public void UpdateMovement(float targetSpeed)
    {
        Vector2 inputMovement = _controller.Input.MovementInput;
        
        if (inputMovement.magnitude < 0.1f)
        {
            // no significant input -> smooth stop
            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * _stats.StopSpeed);
            
            // stop to prevent infinite tiny movements
            if (velocity.magnitude < 0.1f)
            {
                velocity = Vector3.zero;
            }
            return;
        }

        Vector3 moveDirection = GetWorldInputDirection(inputMovement);
        velocity = Vector3.Lerp(velocity, moveDirection * targetSpeed, Time.deltaTime * 8f);
    }
    
    public void UpdateGroundMovement(float targetSpeed)
    {
        UpdateMovement(targetSpeed);
    }
    
    public void UpdateAirMovement()
    {
        // read input & get world‑space direction
        Vector2 input2D = _controller.Input.MovementInput;
        Vector3 moveDir = GetWorldInputDirection(input2D);

        // split off the horizontal velocity
        Vector3 horiz = new Vector3(velocity.x, 0f, velocity.z);
        
        Vector3 target = moveDir * _stats.MaxAirSpeed;

        if (moveDir.magnitude > 0.1f)
        {
            // “snap” on big reversals for crisp turns
            //if (Vector3.Dot(horiz.normalized, moveDir) < -0.5f)
            //  horiz = Vector3.zero;
            
            float maxDelta = _stats.AirAcceleration * Time.deltaTime;
            //horiz = Vector3.MoveTowards(horiz, target, maxDelta);
            horiz = Vector3.Lerp(horiz, target, _stats.AirResponsiveness * Time.deltaTime);
        }
        else
        {
            // if no input, bleed off with air friction
            horiz *= (1f - _stats.AirFriction * Time.deltaTime);
        }
        
        horiz = Vector3.ClampMagnitude(horiz, _stats.MaxAirSpeed);
        
        velocity = new Vector3(horiz.x, velocity.y, horiz.z);
    }

    private Vector3 GetWorldInputDirection(Vector2 inputMovement)
    {
        if (inputMovement.magnitude < 0.1f)
            return Vector3.zero;
            
        Vector3 cameraForward = GetCameraForward();
        Vector3 cameraRight = GetCameraRight();
        
        Vector3 worldDirection = (cameraForward * inputMovement.y + cameraRight * inputMovement.x).normalized;
        return worldDirection;
    }
    
    private Vector3 GetCameraForward()
    {
        Vector3 forward = _controller.PlayerCamera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }
    
    private Vector3 GetCameraRight()
    {
        Vector3 right = _controller.PlayerCamera.transform.right;
        right.y = 0;
        return right.normalized;
    }
    
    public void ApplyGravity()
    {
        if (!_controller.IsGrounded)
        {
            // Floaty gravity
            float gravityMultiplier = (yVelocity > 0 && !_controller.Input.JumpInput) ? _stats.FallGravityMultiplier : 1f;
            yVelocity += _stats.Gravity * gravityMultiplier * Time.deltaTime;
            
            // max velocity
            yVelocity = Mathf.Max(yVelocity, _stats.TerminalVelocity);
        }
        else if (yVelocity < 0)
        {
            yVelocity = -2f; // Small downward force to stay grounded
        }
    }
    
    public void ApplyMovement()
    {
        Vector3 finalMovement = (velocity + Vector3.up * yVelocity) * Time.deltaTime;
        _characterController.Move(finalMovement);
    }
    
    // Character height control
    public void SetCharacterHeight(float targetHeight)
    {
        _characterController.height = Mathf.Lerp(_characterController.height, targetHeight, Time.deltaTime * 10f);
        
        // Adjust center to keep feet on ground
        Vector3 center = _characterController.center;
        center.y = targetHeight * 0.5f;
        _characterController.center = center;
    }
    
    // Direct velocity control for states
    public void SetVelocity(Vector3 newVelocity) => velocity = newVelocity;
    public void SetYVelocity(float newYVelocity) => yVelocity = newYVelocity;
    public void AddVelocity(Vector3 additionalVelocity) => velocity += additionalVelocity;
    public void AddYVelocity(float additionalYVelocity) => yVelocity += additionalYVelocity;
    
    // Utility methods
    public void ResetVelocity() => velocity = Vector3.zero;
    public void ResetYVelocity() => yVelocity = 0f;
    public Vector3 GetMovementDirection() => velocity.normalized;
    public void ApplyForce(Vector3 force) => velocity += force;
}