using UnityEngine;

[System.Serializable]
public class VerticalMovementData
{
    private PlayerController _controller;
    private PlayerStatsSO _stats;
    
    [SerializeField] private float _yVelocity;
    [SerializeField] private float _jumpBufferCounter;
    [SerializeField] private bool _hasUsedGroundJump;
    [SerializeField] private int _remainingAirJumps;
    [SerializeField] private float _jumpHoldTimer;
    [SerializeField] private bool _airJumpInputConsumed;
    
    public float YVelocity => _yVelocity;
    public bool IsJumping => _yVelocity > 0f;
    public bool HasUsedGroundJump => _hasUsedGroundJump;
    public int RemainingAirJumps => _remainingAirJumps;

    public void Initialize(PlayerController controller)
    {
        _controller = controller;
        _stats = controller.Stats;
        ResetJumpData();
    }

    public void UpdateJumpBuffer()
    {
        if (!_controller.Input.JumpHeld)
        {
            _airJumpInputConsumed = false;
        }
        
        if (_stats.CanAutoJump)
        {
            if (_controller.Input.JumpPressed || _controller.Input.JumpHeld)
            {
                _jumpBufferCounter = _stats.JumpBufferTime;
            } 
            else 
            {
                _jumpBufferCounter -= Time.deltaTime;
            }
        }
        else 
        {
            if (_controller.Input.JumpPressed)
            {
                _jumpBufferCounter = _stats.JumpBufferTime;
            }
            else
            {
                _jumpBufferCounter -= Time.deltaTime;
            }
        }
    }

    public bool CanJump()
    {
        if (_controller.IsGrounded)
        {
            bool hasJumpData = !_hasUsedGroundJump && _jumpBufferCounter > 0f;
            if (!hasJumpData) return false;
        
            // check stamina
            if (!_stats.AllowJumpWithoutStamina)
            {
                return _controller.StaminaComponent.CanAfford(_stats.JumpStaminaCost);
            }
            return true;
        }
        else
        {
            return _remainingAirJumps > 0;
        }
    }

    public bool CanAirJump()
    {
        bool hasAirJumps = !_controller.IsGrounded && _remainingAirJumps > 0;
        if (!hasAirJumps) return false;
    
        // check stamina
        if (!_stats.AllowJumpWithoutStamina)
        {
            return _controller.StaminaComponent.CanAfford(_stats.AirJumpStaminaCost);
        }
        return true;
    }

    public bool ShouldTriggerAirJump()
    {
        if (!CanAirJump()) return false;
        
        // If jump was just pressed, always allow air jump
        if (_controller.Input.JumpPressed)
        {
            return true;
        }
        
        // If jump is being held but we haven't consumed this input for an air jump yet
        if (_controller.Input.JumpHeld && !_airJumpInputConsumed)
        {
            return true;
        }
        
        return false;
    }

    public void ExecuteAirJump()
    {
        if (!CanAirJump()) return;
        
        float jumpVelocity = Mathf.Sqrt(_stats.JumpForce * -2f * _stats.Gravity);
        _yVelocity = jumpVelocity;
        
        // Reset jump hold timer for new air jump
        _jumpHoldTimer = 0f;
        
        _remainingAirJumps--;
        _airJumpInputConsumed = true;
        
        if (!_stats.AllowJumpWithoutStamina)
        {
            _controller.StaminaComponent.TryUse(_stats.AirJumpStaminaCost);
        }
        
        //Debug.Log($"Air jump executed. Remaining: {_remainingAirJumps}");
    }
    
    public void ExecuteJump()
    {
        if (!CanJump()) return;
        
        float jumpVelocity = Mathf.Sqrt(_stats.JumpForce * -2f * _stats.Gravity);
        _yVelocity = jumpVelocity;
        
        _jumpHoldTimer = 0f;
        
        if (_controller.IsGrounded)
        {
            _hasUsedGroundJump = true;
            
            if (!_stats.AllowJumpWithoutStamina)
            {
                _controller.StaminaComponent.TryUse(_stats.JumpStaminaCost);
            }
            
            Debug.Log("Ground jump executed");
        }
        else
        {
            _remainingAirJumps--;
            _airJumpInputConsumed = true;
            
            if (!_stats.AllowJumpWithoutStamina)
            {
                _controller.StaminaComponent.TryUse(_stats.AirJumpStaminaCost);
            }
            
            Debug.Log($"Air jump executed. Remaining: {_remainingAirJumps}");
        }
        
        _jumpBufferCounter = 0f;
    }

    public void ApplyGravity()
    {
        if (_controller.IsGrounded)
        {
            if (_yVelocity < 0f)
                _yVelocity = -2f;
            return;
        }
    
        float baseGravity = _stats.Gravity;
        float gravityToApply;
        
        if (_yVelocity > 0f) // Rising (jumping up)
        {
            if (_controller.Input.JumpHeld)
            {
                _jumpHoldTimer += Time.deltaTime;
            }
            
            bool canHoldJump = _controller.Input.JumpHeld && _jumpHoldTimer < _stats.MaxJumpHoldTime;
            
            if (canHoldJump)
            {
                // Apply reduced gravity while holding jump
                gravityToApply = baseGravity;
            }
            else
            {
                // Apply normal gravity after releasing jump
                gravityToApply = baseGravity * _stats.LowJumpMultiplier;
            }
        }
        else // Falling
        {
            // Use stronger gravity when falling
            gravityToApply = baseGravity * _stats.FallGravityMultiplier;
        }
    
        _yVelocity += gravityToApply * Time.deltaTime;
    
        // Clamp to terminal velocity
        _yVelocity = Mathf.Max(_yVelocity, _stats.TerminalVelocity);
    }

    public void SetYVelocity(float velocity)
    {
        _yVelocity = velocity;
    }

    public void ResetYVelocity()
    {
        _yVelocity = 0f;
    }

    public void ResetJumpData()
    {
        _hasUsedGroundJump = false;
        _remainingAirJumps = _stats.MaxAirJumps;
        _jumpBufferCounter = 0f;
        _jumpHoldTimer = 0f;
        _airJumpInputConsumed = false;
    }

    public void OnLanded()
    {
        ResetJumpData();
        // Reset Y velocity when landing
        if (_yVelocity < 0f)
            _yVelocity = 0f;
    }
}