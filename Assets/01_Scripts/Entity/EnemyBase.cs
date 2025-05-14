using UnityEngine;

public abstract class EnemyBase : Entity, IDamageable
{
    [SerializeField] private EnemyStatsSO _enemyStats;

    public Animator Animator;

    protected void Awake()
    {
        Animator = GetComponent<Animator>();
        HealthComponent = GetComponent<HealthComponent>();
    }
    
    public virtual void ApplyDamage(float damage) { }
}