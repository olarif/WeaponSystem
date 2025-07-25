using UnityEngine;

[CreateAssetMenu(fileName = "New Player Stats", menuName = "Player/Player Stats")]
public class PlayerStatsSO : ScriptableObject
{
    [Header("LAYERS")]
    public LayerMask PlayerLayer;
    public LayerMask GroundLayer;
    
    [Header("Basic Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    
    [Header("Movement Responsiveness")]
    [SerializeField] private float groundResponsiveness = 15f;
    [SerializeField] private float stopSpeed = 12f;
    
    [Header("Air Movement")]
    [SerializeField] private float airAcceleration = 8f;
    [SerializeField] private float airResponsiveness = 8f;
    [SerializeField] private float maxAirSpeed = 7f;
    [SerializeField] private float airFriction = 1f;
    
    [Header("Gravity & Jumping")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float fallGravityMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 3f;
    [SerializeField] private float terminalVelocity = -20f;
    [SerializeField] private int maxAirJumps = 1;
    [SerializeField] private float jumpBufferTime = 0.1f;
    
    [Header("Dash System")]
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float airDashUpwardForce = 3f;
    [SerializeField] private bool resetDashOnLand = true;
    
    [Header("Character Dimensions")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    
    [Header("Ground Detection")]
    [SerializeField] private float groundDistance = 0.4f;
    
    [Header("Camera")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float minLookAngle = -80f;
    [SerializeField] private float maxLookAngle = 80f;
    
    // Properties
    public float WalkSpeed => walkSpeed;
    public float SprintSpeed => sprintSpeed;
    public float CrouchSpeed => crouchSpeed;
    
    public float GroundResponsiveness => groundResponsiveness;
    public float StopSpeed => stopSpeed;
    
    public float AirAcceleration => airAcceleration;
    public float AirResponsiveness => airResponsiveness;
    public float MaxAirSpeed => maxAirSpeed;
    public float AirFriction => airFriction;
    
    public float Gravity => gravity;
    public float JumpForce => jumpForce;
    public float FallGravityMultiplier => fallGravityMultiplier;
    public float LowJumpMultiplier => lowJumpMultiplier;
    public float TerminalVelocity => terminalVelocity;
    public int MaxAirJumps => maxAirJumps;
    public float JumpBufferTime => jumpBufferTime;
    
    public float DashForce => dashForce;
    public float DashDuration => dashDuration;
    public float DashCooldown => dashCooldown;
    public float AirDashUpwardForce => airDashUpwardForce;
    public bool ResetDashOnLand => resetDashOnLand;
    
    public float StandingHeight => standingHeight;
    public float CrouchHeight => crouchHeight;
    
    public float GroundDistance => groundDistance;
    
    public float RotationSpeed => rotationSpeed;
    public float MinLookAngle => minLookAngle;
    public float MaxLookAngle => maxLookAngle;
    
    public int TotalJumps => 1 + maxAirJumps;
   
}