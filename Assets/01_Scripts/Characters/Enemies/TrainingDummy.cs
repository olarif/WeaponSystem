using System;
using UnityEngine;

public class TrainingDummy : EnemyBase
{
    public float stunTime = 3f;
    private bool _isStunned = false;
    private CapsuleCollider _collider;

    private void Start()
    {
        _collider = GetComponent<CapsuleCollider>();
    }

    public override void TakeDamage(float damage, DamageType damageType)
    {
        if (_isStunned) return;
        
        HealthComponent.TakeDamage(damage);
        
        Animator.SetTrigger("Damaged");
        
        if (HealthComponent.IsAlive)
        {
            Animator.SetTrigger("Damaged");
        }
        else
        {
            _isStunned = true;
            _collider.enabled = false;
            Animator.SetBool("isDead", true);
            
            Invoke(nameof(ResetDummy), stunTime);
        }
    }

    private void ResetDummy()
    {
        _isStunned = false;
        _collider.enabled = true;
        Animator.SetBool("isDead", false);
        HealthComponent.TryHeal(EnemyStats.MaxHealth);
        //StatusEffectsManager.ClearAllEffects();
    }
}