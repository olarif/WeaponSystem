using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnProjectileAction : WeaponActionData
{
    [Tooltip("The prefab to spawn when firing the weapon.")]
    public GameObject projectilePrefab;
    
    public override void Execute(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        // collect the transforms we should fire from:
        var sources = new List<Transform>();
        foreach (var fp in ctx.FirePoints)
        {
            // fp.IsChildOf() will be true if fp is under leftHand or rightHand
            bool isLeft  = fp.IsChildOf(ctx.leftHand);
            bool isRight = fp.IsChildOf(ctx.rightHand);

            if (binding.fireHand == WeaponDataSO.Hand.Both ||
                (binding.fireHand == WeaponDataSO.Hand.Left  && isLeft) ||
                (binding.fireHand == WeaponDataSO.Hand.Right && isRight))
            {
                sources.Add(fp);
            }
        }

        // fallback if nobody matched
        if (sources.Count == 0)
        {
            Debug.LogWarning($"No FirePoints found for hand {binding.fireHand}; defaulting to right hand.", ctx);
            sources.Add(ctx.rightHand);
        }

        // spawn one projectile per source
        foreach (var fp in sources)
        {
            Vector3 worldPos = fp.position;
            Quaternion worldRot = fp.rotation;

            var projGO = Object.Instantiate(projectilePrefab, worldPos, worldRot);
            if (projGO.TryGetComponent<ProjectileController>(out var ctrl))
                ctrl.Initialize(ctx.gameObject);
        }
    }
}