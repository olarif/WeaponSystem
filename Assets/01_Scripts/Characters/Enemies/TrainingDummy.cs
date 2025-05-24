using System;
using UnityEngine;

public class TrainingDummy : EnemyBase
{
    public float stunTime = 3f;
    
    private bool _isStunned = false;

    public override void TakeDamage(float damage, DamageType damageType)
    {
        if (_isStunned) return;
        
        HealthComponent.TakeDamage(damage);
        
        Animator.SetTrigger("Damaged");
        
        if (HealthComponent.IsAlive())
        {
            Animator.SetTrigger("Damaged");
        }
        else
        {
            _isStunned = true;
            Animator.SetBool("isDead", true);
            Invoke(nameof(ResetDummy), stunTime);
        }
    }

    private void ResetDummy()
    {
        _isStunned = false;
        Animator.SetBool("isDead", false);
        HealthComponent.Heal(HealthComponent.GetMaxHealth());
        //StatusEffectsManager.ClearAllEffects();
    }
}