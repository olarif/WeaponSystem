using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DamageComponent : ProjectileComponent
{
    [Tooltip("Which event(s) to damage on")]
    public List<ProjectileEvent> triggerEvents = new List<ProjectileEvent>
    {
        ProjectileEvent.OnCollision
    };

    [Tooltip("Instant damage amount")]
    public float damageAmount = 0f;

    [Tooltip("Damage per second (if you pick OnTick)")]
    public float damagePerSecond = 0f;

    [Tooltip("If >0, do AOE damage instead of single-target")]
    public float areaOfEffectRadius = 0f;

    [Tooltip("Layers that can be damaged")]
    public LayerMask mask = ~0;

    public override void UpdateComponent(ProjectileRuntimeData data)
    {
        if (triggerEvents.Contains(ProjectileEvent.OnTick) && damagePerSecond > 0f)
        {
            Deal(data, damagePerSecond * Time.deltaTime, null);
        }
    }

    public override ComponentResult OnCollision(
        ProjectileRuntimeData data, CollisionInfo ci)
    {
        if (triggerEvents.Contains(ProjectileEvent.OnCollision)
            && damageAmount > 0f)
        {
            Deal(data, damageAmount, ci.HitObject);
        }
        return ComponentResult.Continue;
    }

    public override void OnDestroy(ProjectileRuntimeData data)
    {
        // run on destroy (e.g. explosion)
        if (triggerEvents.Contains(ProjectileEvent.OnDestroy)
            && damageAmount > 0f)
        {
            // areaOfEffectRadius > 0 → true AOE, otherwise single target
            Deal(data, damageAmount, areaOfEffectRadius > 0f ? null : data.lastCollision.HitObject);
        }
    }

    /// <summary>
    /// If onlyTarget != null, hits that one target. Otherwise, if
    /// areaOfEffectRadius>0, performs an OverlapSphere AOE.
    /// </summary>
    private void Deal(
        ProjectileRuntimeData data,
        float amt,
        GameObject onlyTarget)
    {
        if (areaOfEffectRadius > 0f && onlyTarget == null)
        {
            // AOE
            var hits = Physics.OverlapSphere(
                data.currentPosition,
                areaOfEffectRadius,
                mask);

            foreach (var c in hits)
            {
                if (c.gameObject.TryGetComponent<IDamageable>(out var d))
                    d.TakeDamage(amt, DamageType.Physical);
            }
        }
        else if (onlyTarget != null)
        {
            // Single‐target
            if (onlyTarget.TryGetComponent<IDamageable>(out var d))
                d.TakeDamage(amt, DamageType.Physical);
        }
    }
}
