using UnityEngine;

public class StatusEffectOnHit : OnHitComponent
{
    public StatusEffectDataSO statusEffectData;
    
    public override void OnHit(CollisionInfo info)
    {
        var target = info.HitObject.GetComponent<StatusEffectManager>();
        
        if (target != null)
        {
            target.Apply(statusEffectData, statusEffectData.impactDamage);
        }
        else
        {
            Debug.LogWarning("No StatusEffectManager found on hit object.");
        }
    }
}