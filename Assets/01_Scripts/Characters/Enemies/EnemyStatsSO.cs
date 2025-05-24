using UnityEngine;

[CreateAssetMenu]
public class EnemyStatsSO : ScriptableObject
{
    [Header("LAYERS")]
    
    [Tooltip("Set this to the layer that the player is on")]
    public LayerMask EnemyLayer;
    
    [Header("Player Stats")]
    public float WalkSpeed = 7f;
    public float SprintSpeed = 12f;
    [Tooltip("The time it takes for the player to reach max speed")]
    public float MoveSmoothTime = 10f;
    
}
