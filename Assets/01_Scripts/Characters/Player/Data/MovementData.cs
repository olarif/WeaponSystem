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
    
    [Header("Sprint Buffering")]
    [SerializeField] private float sprintBufferTimer;
    [SerializeField] private float sprintReleaseTimer;
    [SerializeField] private bool isSprintBuffered;
    
    [Header("Air Movement Debug")]
    [SerializeField] private Vector3 lastGroundVelocity;
    [SerializeField] private float currentAirSpeed;
    [SerializeField] private bool wasGroundedLastFrame;
    
    public Vector3 Velocity => velocity;
    public float YVelocity => yVelocity;
    public bool IsMoving => velocity.magnitude > _stats.StopThreshold;
    public bool IsSprinting => isSprintBuffered;
    public float CurrentSpeed => velocity.magnitude;
    public Vector3 LastGroundVelocity => lastGroundVelocity;
    
    public void Initialize(PlayerController controller, CharacterController characterController, PlayerStatsSO stats)
    {
        _controller = controller;
        _characterController = characterController;
        _stats = stats;
        
        velocity = Vector3.zero;
        yVelocity = 0f;
        lastGroundVelocity = Vector3.zero;
        wasGroundedLastFrame = true;
        
        sprintBufferTimer = 0f;
        sprintReleaseTimer = 0f;
        isSprintBuffered = false;
    }

    public void UpdateMovement()
    {
        // Update sprint buffering first
        UpdateSprintBuffering();
        
        // Track grounded state changes
        bool isGrounded = _controller.IsGrounded;
        
        if (isGrounded)
        {
            UpdateGroundMovement();
            
            if (!wasGroundedLastFrame)
            {
                OnLanded();
            }
        }
        else
        {
            UpdateAirMovement();
            
            if (wasGroundedLastFrame)
            {
                OnLeftGround();
            }
        }
        
        // Apply final movement
        ApplyMovementToController();
        
        // Update tracking
        wasGroundedLastFrame = isGrounded;
    }

    private void UpdateSprintBuffering()
    {
        bool sprintInput = _controller.Input.SprintInput;

        if (sprintInput)
        {
            // Sprint input pressed
            sprintBufferTimer += Time.deltaTime;
            sprintReleaseTimer = 0f;

            // Switch to sprint after buffer time
            if (sprintBufferTimer >= _stats.SprintBufferTime)
            {
                isSprintBuffered = true;
            }
        }
        else
        {
            // Sprint input released
            sprintBufferTimer = 0f;

            if (isSprintBuffered)
            {
                sprintReleaseTimer += Time.deltaTime;

                // Switch back to walk after release delay
                if (sprintReleaseTimer >= _stats.SprintReleaseDelay)
                {
                    isSprintBuffered = false;
                }
            }
        }
    }

    private void UpdateGroundMovement()
    {
        Vector2 inputMovement = _controller.Input.MovementInput;
        Vector3 inputDirection = new Vector3(inputMovement.x, 0, inputMovement.y);
        
        // Get desired movement direction in world space
        Vector3 worldInputDirection = CalculateWorldInputDirection(inputDirection);
        
        // Calculate target velocity
        float targetSpeed = GetCurrentTargetSpeed();
        Vector3 targetVelocity = worldInputDirection * targetSpeed;
        
        // Apply proper acceleration/deceleration
        ApplyGroundAcceleration(targetVelocity, worldInputDirection);
        
        // Store for air movement
        lastGroundVelocity = velocity;
    }
    
    private Vector3 CalculateWorldInputDirection(Vector3 inputDirection)
    {
        if (inputDirection.magnitude < 0.1f)
            return Vector3.zero;
            
        Vector3 cameraForward = GetCameraForward();
        Vector3 cameraRight = GetCameraRight();
        
        Vector3 worldDirection = (cameraForward * inputDirection.z + cameraRight * inputDirection.x).normalized;
        return worldDirection;
    }
    
    private float GetCurrentTargetSpeed()
    {
        // Use buffered sprint state instead of state machine
        return isSprintBuffered ? _stats.SprintSpeed : _stats.WalkSpeed;
    }
    
    private Vector3 CalculateGroundTargetVelocity(Vector3 moveDirection, float targetSpeed)
    {
        Vector3 cameraForward = GetCameraForward();
        Vector3 cameraRight = GetCameraRight();
        
        Vector3 targetVelocity = (cameraForward * moveDirection.z + cameraRight * moveDirection.x) * targetSpeed;
        return targetVelocity;
    }
    
    private void ApplyGroundAcceleration(Vector3 targetVelocity, Vector3 inputDirection)
    {
        if (inputDirection.magnitude > 0.1f)
        {
            // Player is giving input - accelerate towards target
            ApplyAcceleration(targetVelocity);
        }
        else
        {
            // No input - decelerate
            ApplyDeceleration();
        }
    }
    
    private void ApplyAcceleration(Vector3 targetVelocity)
    {
        Vector3 velocityDifference = targetVelocity - velocity;
        
        // Check if we're changing direction or speeding up
        float dot = Vector3.Dot(velocity.normalized, targetVelocity.normalized);
        
        float accelerationRate;
        if (dot < 0.5f) // Changing direction significantly
        {
            accelerationRate = _stats.DirectionChangeSpeed;
        }
        else // Same direction, just changing speed
        {
            accelerationRate = _stats.Acceleration;
        }
        
        // Apply acceleration with proper physics
        Vector3 acceleration = velocityDifference.normalized * (accelerationRate * Time.deltaTime);
        
        // Don't overshoot the target
        if (acceleration.magnitude > velocityDifference.magnitude)
        {
            velocity = targetVelocity;
        }
        else
        {
            velocity += acceleration;
        }
    }
    
    private void ApplyDeceleration()
    {
        // Apply deceleration until we reach stop threshold
        float currentSpeed = velocity.magnitude;
        
        if (currentSpeed > _stats.StopThreshold)
        {
            float decelerationAmount = _stats.Deceleration * Time.deltaTime;
            float newSpeed = Mathf.Max(0f, currentSpeed - decelerationAmount);
            
            if (newSpeed < _stats.StopThreshold)
            {
                velocity = Vector3.zero;
            }
            else
            {
                velocity = velocity.normalized * newSpeed;
            }
        }
        else
        {
            velocity = Vector3.zero;
        }
    }
    
    private void UpdateAirMovement()
    {
        Vector2 inputMovement = _controller.Input.MovementInput;
        
        if (inputMovement.magnitude > 0.1f)
        {
            Vector3 inputDirection = GetAirInputDirection(inputMovement);
            ApplyAirAcceleration(inputDirection);
            
            if (_stats.EnableStrafeJumping)
            {
                ApplyStrafeJumping(inputDirection);
            }
            
            LimitAirSpeed();
        }
        else
        {
            ApplyAirFriction();
        }
        
        currentAirSpeed = velocity.magnitude;
    }
    
    private Vector3 GetAirInputDirection(Vector2 inputMovement)
    {
        Vector3 cameraForward = GetCameraForward();
        Vector3 cameraRight = GetCameraRight();
        
        Vector3 moveDirection = new Vector3(inputMovement.x, 0, inputMovement.y).normalized;
        Vector3 inputDirection = (cameraForward * moveDirection.z + cameraRight * moveDirection.x).normalized;
        
        return inputDirection;
    }
    
    private void ApplyAirAcceleration(Vector3 inputDirection)
    {
        Vector3 currentVelNormalized = velocity.normalized;
        float dot = Vector3.Dot(currentVelNormalized, inputDirection);
        
        float accelerationMultiplier = Mathf.Lerp(1f, 0.3f, Mathf.Max(0, dot));
        Vector3 acceleration = inputDirection * (_stats.AirAcceleration * accelerationMultiplier * _stats.AirControl);
        velocity += acceleration * Time.deltaTime;
    }
    
    private void ApplyStrafeJumping(Vector3 inputDirection)
    {
        Vector3 currentVelNormalized = velocity.normalized;
        Vector3 perpendicular = Vector3.Cross(Vector3.up, currentVelNormalized).normalized;
        
        float strafeAlignment = Mathf.Abs(Vector3.Dot(inputDirection, perpendicular));
        
        if (strafeAlignment > 0.7f)
        {
            float speedGain = _stats.AirAcceleration * strafeAlignment * (_stats.StrafeJumpMultiplier - 1f);
            velocity += inputDirection * (speedGain * Time.deltaTime);
        }
    }
    
    private void LimitAirSpeed()
    {
        float maxSpeed = Mathf.Max(_stats.MaxAirSpeed, lastGroundVelocity.magnitude);
        
        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }
    }
    
    private void ApplyAirFriction()
    {
        velocity *= Mathf.Max(0, 1f - _stats.AirFriction * Time.deltaTime);
    }
    
    private void OnLeftGround()
    {
        //Debug.Log($"Left ground with velocity: {velocity.magnitude:F2}");
        lastGroundVelocity = velocity;
    }
    
    private void OnLanded()
    {
        //Debug.Log($"Landed with velocity: {velocity.magnitude:F2}");
        // velocity *= 0.9f;
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
    
    private void ApplyMovementToController()
    {
        Vector3 finalMovement = (velocity + Vector3.up * yVelocity) * Time.deltaTime;
        _characterController.Move(finalMovement);
    }
    
    public void ApplyGravity()
    {
        if (!_controller.IsGrounded)
        {
            float gravityMultiplier = ShouldApplyFastFall() ? 2f : 1f;
            yVelocity += _stats.Gravity * gravityMultiplier * Time.deltaTime;
        }
        else if (yVelocity < 0)
        {
            yVelocity = -2f;
        }
    }
    
    private bool ShouldApplyFastFall()
    {
        return !_controller.Input.JumpInput && yVelocity > 0;
    }
    
    public void SetYVelocity(float newYVelocity) => yVelocity = newYVelocity;
    public void ResetVelocity() => velocity = Vector3.zero;
    public void ResetYVelocity() => yVelocity = 0f;
    
    public void ApplyForce(Vector3 force)
    {
        velocity += force;
    }
    
    public Vector3 GetMovementDirection()
    {
        return velocity.normalized;
    }
}