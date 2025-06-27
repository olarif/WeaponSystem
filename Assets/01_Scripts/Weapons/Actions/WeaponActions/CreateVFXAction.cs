using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Instantiates a VFX prefab at the weapon's first fire point when executed.
/// </summary>
[System.Serializable]
public class CreateVFXAction : IWeaponAction
{
    [Tooltip("VFX prefab to spawn")]
    public GameObject vfxPrefab;
    [Tooltip("Duration before VFX is destroyed")]
    public float lifetime = 5f;
    [Tooltip("Scale applied to spawned VFX")]
    public Vector3 spawnSize = Vector3.one;

    public void Execute(WeaponContext ctx, InputBindingData binding, ActionBindingData actionBinding)
    {
        if (ctx == null || ctx.FirePoints == null || ctx.FirePoints.Count == 0) return;

        SpawnVFX(ctx.FirePoints[0]);
    }

    private void SpawnVFX(Transform firePoint)
    {
        if (vfxPrefab == null) return;

        GameObject vfx = Object.Instantiate(
            vfxPrefab,
            firePoint.position,
            Quaternion.LookRotation(firePoint.forward)
        );
        
        vfx.transform.localScale = spawnSize;

        // schedule destruction after lifetime seconds
        if (lifetime > 0f)
            Object.Destroy(vfx, lifetime);
    }
}