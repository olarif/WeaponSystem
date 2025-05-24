using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnProjectileAction : WeaponActionData
{
    [Tooltip("The prefab to spawn when firing the weapon.")]
    public GameObject projectilePrefab;
    
    public override void OnPress(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        SpawnProjectile(ctx, binding);
    }
    
    public override void OnRelease(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        SpawnProjectile(ctx, binding);
    }

    private void SpawnProjectile(WeaponContext ctx, WeaponDataSO.InputBinding b)
    {
        var sources = new List<Transform>();
        foreach (var fp in ctx.FirePoints)
        {
            bool isLeft  = fp.IsChildOf(ctx.leftHand);
            bool isRight = fp.IsChildOf(ctx.rightHand);
            if (b.fireHand == WeaponDataSO.Hand.Both ||
                (b.fireHand == WeaponDataSO.Hand.Left  && isLeft) ||
                (b.fireHand == WeaponDataSO.Hand.Right && isRight))
            {
                sources.Add(fp);
            }
        }

        if (sources.Count == 0) sources.Add(ctx.rightHand);

        foreach (var fp in sources)
        {
            var go = Object.Instantiate(projectilePrefab, fp.position, fp.rotation);
            if (go.TryGetComponent<ProjectileController>(out var pc))
                pc.Initialize(ctx.gameObject);
        }
    }
}