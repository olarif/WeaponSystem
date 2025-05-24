using System;
using UnityEngine;

[Serializable]
public class MeleeDamageAction : IWeaponAction
{
    public DamageType damageType;
    public float damage = 5f;
    public float range = 2.5f;
    
    public void Execute(WeaponContext ctx, InputBindingData binding, ActionBindingData actionBinding)
    {
        Collider[] hits = Physics.OverlapSphere(ctx.transform.position, range);
        foreach (var c in hits)
        {
            if (c.TryGetComponent<IDamageable>(out var d))
                d.TakeDamage(damage, damageType);
        }
    }
}