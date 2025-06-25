using UnityEngine;

public abstract class EnemyBase : Entity, IDamageable
{
    protected HealthComponent HealthComponent;
    
    protected StatusEffectManager StatusEffectsManager;
    protected Animator Animator;

    protected void Awake()
    {
        Animator = GetComponent<Animator>();
        HealthComponent = GetComponent<HealthComponent>();
        StatusEffectsManager = GetComponent<StatusEffectManager>();
    }
    
    public virtual void TakeDamage(float damage, DamageType damageType) { }
}