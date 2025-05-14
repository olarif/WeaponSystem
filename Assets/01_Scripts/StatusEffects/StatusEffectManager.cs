using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    List <StatusEffect> activeEffects = new();
    
    public void Apply(StatusEffectDataSO statusEffectData, float impactDamage = 0f)
    {
        StatusEffect newEffect = new(statusEffectData, this);
        activeEffects.Add(newEffect);
        
        if (impactDamage > 0f)
        {
            var damageable = GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.ApplyDamage(impactDamage);
            } 
            else
            {
                Debug.LogWarning("No IDamageable component found on the target.");
            }
        }
    }

    void Update()
    {
         if(activeEffects.Count == 0 ) return;
         
        // Update all active effects
        foreach (var effect in activeEffects.ToArray())
        {
            effect.Tick(Time.deltaTime);
            
            if (effect.IsComplete)
            {
                activeEffects.Remove(effect);
            }
        }
    }
    
    public void ClearAllEffects()
    {
        activeEffects.Clear();
    }
}