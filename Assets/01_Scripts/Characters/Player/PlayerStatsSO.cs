using UnityEngine;

[CreateAssetMenu]
public class PlayerStatsSO : ScriptableObject
{
    [Header("LAYERS")]
    public LayerMask PlayerLayer;
    public LayerMask GroundLayer;
    
    
    [Header("Ground Movement")]
    public float WalkSpeed = 7f;
    public float SprintSpeed = 12f;
    
    [Tooltip("How quickly player accelerates to target speed")]
    public float Acceleration = 10f;
    
    [Tooltip("How quickly player decelerates when stopping")]
    public float Deceleration = 15f;
    
    [Tooltip("How quickly player can change direction")]
    public float DirectionChangeSpeed = 20f;
    
    [Tooltip("Minimum speed before considering player stopped")]
    public float StopThreshold = 0.1f;
    
    
    [Header("Sprint Settings")]
    [Tooltip("How long sprint input must be held before switching to sprint")]
    public float SprintBufferTime = 0.1f;
    
    [Tooltip("How long after releasing sprint before switching back to walk")]
    public float SprintReleaseDelay = 0.05f;

    
    [Header("Air Movement")]
    public float AirControl = 0.3f;
    public float AirAcceleration = 10f;
    public float MaxAirSpeed = 15f;
    public float AirFriction = 0.1f;
    public bool EnableStrafeJumping = true;
    public float StrafeJumpMultiplier = 1.2f;
    
    [Header("Jump Stats")]
    public float Gravity = -20f;
    public float JumpForce = 3f;
    public float JumpBufferTime = 0.05f;
    public float GroundDistance = 0.4f;
    
    [Header("Camera Stats")]
    public float RotationSpeed = 35f;
    public float MinLookAngle = -90f;
    public float MaxLookAngle = 90f;

   


}
