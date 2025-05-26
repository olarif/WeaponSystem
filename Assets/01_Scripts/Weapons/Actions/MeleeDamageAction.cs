using System;
using UnityEngine;

[Serializable]
public class MeleeDamageAction : IWeaponAction
{
    public DamageType damageType = DamageType.Physical;
    public LayerMask targetLayerMask = ~0;
    public float       damage     = 5f;
    public float       range      = 2.5f;

    public void Execute(WeaponContext ctx, InputBindingData b, ActionBindingData a)
    {
        Vector3 origin = ctx.transform.position;
        var hits = Physics.OverlapSphere(origin, range, targetLayerMask);

        foreach (var c in hits)
        {
            // skip self
            if (c.transform.IsChildOf(ctx.transform)) continue;

            if (c.TryGetComponent<IDamageable>(out var d))
                d.TakeDamage(damage, damageType);
        }
    }
}