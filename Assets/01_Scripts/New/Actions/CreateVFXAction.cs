using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CreateVFXAction : WeaponActionData
{
    public GameObject vfxPrefab;
    public float duration = 1f;

    public override void Execute(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        if (vfxPrefab == null) return;

        // Instantiate the VFX prefab at the weapon's position and rotation
        var vfxInstance = Object.Instantiate(vfxPrefab, ctx.transform.position, ctx.transform.rotation);
        
    }
}