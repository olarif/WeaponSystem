using UnityEngine;

[System.Serializable]
public class GroundCheckData
{
    private Transform _groundCheck;
    private PlayerStatsSO _stats;
    
    public bool IsGrounded { get; private set; }

    public void Initialize(Transform groundCheck, PlayerStatsSO stats)
    {
        _groundCheck = groundCheck;
        _stats = stats;
    }

    public void UpdateGroundCheck()
    {
        if (_groundCheck == null) return;
        
        IsGrounded = Physics.CheckSphere(
            _groundCheck.position, 
            _stats.GroundDistance, 
            _stats.GroundLayer
        );
    }
}