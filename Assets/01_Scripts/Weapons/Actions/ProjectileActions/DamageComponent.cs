using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DamageComponent : ProjectileComponent
{
    public List<ProjectileEvent> triggerEvents = new List<ProjectileEvent>
    {
        ProjectileEvent.OnCollision,
        ProjectileEvent.OnTick,
        ProjectileEvent.OnDestroy
    };

    public float damageAmount = 0f;      // instant damage
    public float damagePerSecond = 0f;   // ticks damage
    public float areaOfEffectRadius = 0f; // AOE radius; 0 = single target
    public LayerMask mask = ~0;          // damageable layers

    public override void UpdateComponent(ProjectileRuntimeData data)
    {
        if (triggerEvents.Contains(ProjectileEvent.OnTick) && damagePerSecond > 0f)
            Deal(data, damagePerSecond * Time.deltaTime, null);
    }

    public override ComponentResult OnCollision(ProjectileRuntimeData data, CollisionInfo ci)
    {
        if (triggerEvents.Contains(ProjectileEvent.OnCollision) && damageAmount > 0f)
            Deal(data, damageAmount, ci.HitObject);
        return ComponentResult.Continue;
    }

    public override void OnDestroy(ProjectileRuntimeData data)
    {
        if (triggerEvents.Contains(ProjectileEvent.OnDestroy) && damageAmount > 0f)
            Deal(data, damageAmount, areaOfEffectRadius > 0f ? null : data.lastCollision.HitObject);
    }

    // Applies damage: single target or AOE
    private void Deal(ProjectileRuntimeData data, float amt, GameObject onlyTarget)
    {
        if (onlyTarget != null)
        {
            TryDamage(onlyTarget, amt);
        }
        else if (areaOfEffectRadius > 0f)
        {
            var hits = Physics.OverlapSphere(data.currentPosition, areaOfEffectRadius, mask);
            foreach (var c in hits)
                TryDamage(c.gameObject, amt);
        }
    }

    private void TryDamage(GameObject target, float amt)
    {
        if (target.TryGetComponent<IDamageable>(out var dmg))
            dmg.TakeDamage(amt, DamageType.Physical);
    }
}