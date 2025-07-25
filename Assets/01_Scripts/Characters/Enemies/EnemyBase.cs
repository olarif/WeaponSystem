using UnityEngine;

public abstract class EnemyBase : Entity, IDamageable
{
    public EnemyStatsSO EnemyStats;
    protected EnemyHealthComponent HealthComponent;
    
    protected StatusEffectManager StatusEffectsManager;
    protected Animator Animator;

    protected void Awake()
    {
        Animator = GetComponent<Animator>();
        HealthComponent = GetComponent<EnemyHealthComponent>();
        StatusEffectsManager = GetComponent<StatusEffectManager>();
    }
    
    public virtual void TakeDamage(float damage, DamageType damageType) { }
}