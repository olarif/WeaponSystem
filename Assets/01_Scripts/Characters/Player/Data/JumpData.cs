using UnityEngine;

[System.Serializable]
public class JumpData
{
    private PlayerController _controller;
    private PlayerStatsSO _stats;
    
    [SerializeField] private float jumpBufferCounter;
    
    public bool IsJumping => _controller.MovementData.YVelocity > 0f;

    public void Initialize(PlayerController controller)
    {
        _controller = controller;
        _stats = controller.Stats;
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
        return _controller.IsGrounded && jumpBufferCounter > 0f;
    }
    
    public void ExecuteJump()
    {
        if (!CanJump()) return;
        
        float jumpVelocity = Mathf.Sqrt(_stats.JumpForce * -2f * _stats.Gravity);
        _controller.MovementData.SetYVelocity(jumpVelocity);
        jumpBufferCounter = 0f;
    }
}