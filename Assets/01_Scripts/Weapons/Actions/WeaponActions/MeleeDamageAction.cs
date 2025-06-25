using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class MeleeDamageAction : IWeaponAction
{
    public DamageType damageType = DamageType.Physical;
    public LayerMask targetLayerMask = ~0;
    public float       damage     = 5f;
    public float       hitRange   = 2.5f;
    public float       hitRadius  = 2.5f;

    public void Execute(WeaponContext ctx, InputBindingData b, ActionBindingData a)
    {
        var cam = ctx.PlayerCamera;
        Vector3 origin    = cam.transform.position;
        Vector3 direction = cam.transform.forward;
        
        var hits = Physics.SphereCastAll(origin, hitRadius, direction, hitRange, targetLayerMask)
            .OrderBy(h => h.distance);

        foreach (var h in hits)
        {
            var c = h.collider;
            if (c.transform.IsChildOf(ctx.transform)) continue;

            if (c.TryGetComponent<IDamageable>(out var d))
                d.TakeDamage(damage, damageType);
        }
    }
}