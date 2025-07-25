using UnityEngine;

[System.Serializable]
public class JumpData
{
    private PlayerController _controller;
    private PlayerStatsSO _stats;
    
    [SerializeField] private float jumpBufferCounter;
    [SerializeField] private bool hasUsedGroundJump = false;
    [SerializeField] private int remainingAirJumps = 0;
    
    public bool IsJumping => _controller.MovementData.YVelocity > 0f;
    public bool HasUsedGroundJump => hasUsedGroundJump;
    public int RemainingAirJumps => remainingAirJumps;

    public void Initialize(PlayerController controller)
    {
        _controller = controller;
        _stats = controller.Stats;
        ResetJumps();
    }

    public void UpdateJumpBuffer()
    {
        if (_controller.Input.JumpInput)
        {
            jumpBufferCounter = _stats.JumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }
    
    public bool CanJump()
    {
        if (_controller.IsGrounded)
        {
            // Can always ground jump if haven't used it yet and have buffer time
            return !hasUsedGroundJump && jumpBufferCounter > 0f;
        }
        else
        {
            // Can air jump if we have air jumps remaining
            return remainingAirJumps > 0;
        }
    }
    
    public void ExecuteJump()
    {
        if (!CanJump()) return;
        
        float jumpVelocity = Mathf.Sqrt(_stats.JumpForce * -2f * _stats.Gravity);
        _controller.MovementData.SetYVelocity(jumpVelocity);
        
        if (_controller.IsGrounded)
        {
            // Use ground jump
            hasUsedGroundJump = true;
            Debug.Log("Ground jump");
        }
        else
        {
            // Use air jump
            remainingAirJumps--;
            Debug.Log($"Air jump. Remaining air jumps: {remainingAirJumps}");
        }
        
        jumpBufferCounter = 0f;
    }
    
    public void ResetJumps()
    {
        hasUsedGroundJump = false;
        remainingAirJumps = _stats.MaxAirJumps;
    }
    
    public void OnLanded()
    {
        ResetJumps();
    }
    
    public bool CanGroundJump()
    {
        return _controller.IsGrounded && !hasUsedGroundJump && jumpBufferCounter > 0f;
    }
    
    public bool CanAirJump()
    {
        return !_controller.IsGrounded && remainingAirJumps > 0;
    }
}