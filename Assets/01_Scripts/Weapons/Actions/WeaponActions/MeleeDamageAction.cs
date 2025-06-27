using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Performs a melee attack that applies damage to targets within a specified range and radius
/// </summary>
[Serializable]
public class MeleeDamageAction : IWeaponAction
{
    [Tooltip("Type of damage to apply")]
    public DamageType damageType = DamageType.Physical;
    [Tooltip("Layers eligible for damage")]
    public LayerMask targetLayerMask = ~0;
    [Tooltip("Damage amount per hit")]
    public float damage = 5f;
    [Tooltip("Maximum distance to check")]
    public float hitRange = 2.5f;
    [Tooltip("Radius of the spherecast")]
    public float hitRadius = 2.5f;

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