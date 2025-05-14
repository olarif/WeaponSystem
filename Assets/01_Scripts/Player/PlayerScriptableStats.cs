using UnityEngine;

[CreateAssetMenu]
public class PlayerScriptableStats : ScriptableObject
{
    [Header("LAYERS")]
    
    [Tooltip("Set this to the layer that the player is on")]
    public LayerMask PlayerLayer;
    
    [Tooltip("Set this to the walkable layer")]
    public LayerMask GroundLayer;
    
    [Header("Player Stats")]
    public float WalkSpeed = 7f;
    public float SprintSpeed = 12f;
    [Tooltip("The time it takes for the player to reach max speed")]
    public float MoveSmoothTime = 10f;
    
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
