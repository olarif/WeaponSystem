using UnityEngine;

[System.Serializable]
public class HorizontalMovementData
{
    private PlayerController _controller;
    private CharacterController _characterController;
    private PlayerStatsSO _stats;
    
    [SerializeField] private Vector3 _horizontalVelocity;
    [SerializeField] private Vector3 _externalForces;
    
    public Vector3 HorizontalVelocity => _horizontalVelocity;
    public float Speed => _horizontalVelocity.magnitude;
    public bool IsMoving => Speed > 0.1f;
    public Vector3 ExternalForces => _externalForces;

    public void Initialize(PlayerController controller, CharacterController characterController, PlayerStatsSO stats)
    {
        _controller = controller;
        _characterController = characterController;
        _stats = stats;
        _horizontalVelocity = Vector3.zero;
        _externalForces = Vector3.zero;
    }

    // Ground movement with different speeds for walk/sprint/crouch
    public void UpdateGroundMovement(float targetSpeed)
    {
        Vector2 input = _controller.Input.MovementInput;
        
        if (input.magnitude < 0.1f)
        {
            // Decelerate when no input
            _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, Vector3.zero, 
                Time.deltaTime * _stats.StopSpeed);
            
            if (_horizontalVelocity.magnitude < 0.1f)
                _horizontalVelocity = Vector3.zero;
            return;
        }

        Vector3 worldDirection = GetWorldDirection(input);
        Vector3 targetVelocity = worldDirection * targetSpeed;
        
        _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, targetVelocity, 
            Time.deltaTime * _stats.GroundResponsiveness);
    }

    // Air movement with different physics
    public void UpdateAirMovement()
    {
        Vector2 input = _controller.Input.MovementInput;
        Vector3 worldDirection = GetWorldDirection(input);

        Vector3 current = _horizontalVelocity;

        if (worldDirection.magnitude > 0.1f)
        {
            // snap reverse optional for instant turning (feels off)
            //if (Vector3.Dot(current.normalized, worldDir) < -0.5f)
            //    current = Vector3.zero;
            
            Vector3 target = worldDirection * _stats.MaxAirSpeed;
            
            float accelerationStep = _stats.AirAcceleration * Time.deltaTime;
            //_horizontalVelocity = Vector3.MoveTowards(current, target, accelerationStep);
            
            _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, target, _stats.AirResponsiveness * Time.deltaTime);
        }
        else
        {
            // Apply air friction when no input
            _horizontalVelocity *= (1f - _stats.AirFriction * Time.deltaTime);
        }

        // Clamp to max air speed
        _horizontalVelocity = Vector3.ClampMagnitude(_horizontalVelocity, _stats.MaxAirSpeed);
    }
    
    public void ApplyForce(Vector3 force)
    {
        _externalForces += force;
    }
    
    public void UpdateExternalForces()
    {
        _externalForces = Vector3.Lerp(_externalForces, Vector3.zero, Time.deltaTime * 5f);
        
        if (_externalForces.magnitude < 0.1f)
            _externalForces = Vector3.zero;
    }

    public void ResetVelocity()
    {
        _horizontalVelocity = Vector3.zero;
    }

    public void ResetExternalForces()
    {
        _externalForces = Vector3.zero;
    }

    public Vector3 GetTotalHorizontalVelocity()
    {
        return _horizontalVelocity + _externalForces;
    }

    private Vector3 GetWorldDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f) return Vector3.zero;

        Transform cameraTransform = _controller.PlayerCamera.transform;
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        
        // Remove Y component for horizontal movement
        forward.y = 0f;
        right.y = 0f;
        
        forward.Normalize();
        right.Normalize();

        return (forward * input.y + right * input.x).normalized;
    }
}