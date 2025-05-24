using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CreateVFXAction : IWeaponAction
{
    public GameObject vfxPrefab;
    public float lifetime = 5f;

    public void Execute(WeaponContext ctx, InputBindingData binding, ActionBindingData actionBinding)
    {
        if (ctx == null || ctx.FirePoints == null || ctx.FirePoints.Count == 0) return;

        SpawnVFX(ctx);
    }

    private void SpawnVFX(WeaponContext ctx)
    {
        if (vfxPrefab == null) return;

        var origin = ctx.FirePoints[0].position;
        var dir    = ctx.FirePoints[0].forward;

        var go = Object.Instantiate(vfxPrefab, origin, Quaternion.LookRotation(dir));
        
        if(lifetime <= 0f) return;
            Object.Destroy(go, lifetime);
    }
}