using System;
using UnityEngine;

[Serializable]
public class SpawnProjectileAction : IWeaponAction
{
    public GameObject prefab;
    
    public void Execute(WeaponContext ctx, InputBindingData b, ActionBindingData ab)
    {
        foreach (var fp in ctx.GetFirePointsFor(b.hand))
        {
            var proj = GameObject.Instantiate(prefab, fp.position, fp.rotation);
            if (proj.TryGetComponent<ProjectileController>(out var pc))
                pc.Initialize(ctx.WeaponController.gameObject);
        }
    }
}