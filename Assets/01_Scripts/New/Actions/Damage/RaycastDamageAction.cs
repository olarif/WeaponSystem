using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RaycastDamageAction : IWeaponAction
{
    
    public float maxDistance, damagePerTick, tickRate;
    public DamageType damageType;
    float _last;

    public void Execute(WeaponContext ctx, InputBindingData binding, ActionBindingData actionBinding)
    {
        if (Time.time < _last + tickRate) return;
        _last = Time.time;

        var origin = ctx.FirePoints[0].position;
        var dir    = ctx.FirePoints[0].forward;
        if (Physics.Raycast(origin, dir, out var hit, maxDistance) &&
            hit.collider.TryGetComponent<IDamageable>(out var d))
            d.TakeDamage(damagePerTick, damageType);
    }
}
