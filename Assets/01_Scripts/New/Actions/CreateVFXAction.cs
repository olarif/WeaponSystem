using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CreateVFXAction : WeaponActionData
{
    public GameObject vfxPrefab;
    public float lifetime = 5f;

    public override void OnPress(WeaponContext ctx, WeaponDataSO.InputBinding b)
    {
        SpawnVFX(ctx);
    }
    
    public override void OnRelease(WeaponContext ctx, WeaponDataSO.InputBinding b)
    {
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