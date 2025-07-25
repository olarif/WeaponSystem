using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Stats", menuName = "Enemy/Enemy Stats")]
public class EnemyStatsSO : ScriptableObject
{
    [Header("Max Health & Stamina")]
    [SerializeField] private float maxHealth = 100f;
    
    [Header("Health Regeneration")]
    [SerializeField] private float healthRegenRate = 2f;
    [SerializeField] private float healthRegenDelay = 5f;
    
    public float MaxHealth => maxHealth;
    public float HealthRegenRate => healthRegenRate;
    public float HealthRegenDelay => healthRegenDelay;
}