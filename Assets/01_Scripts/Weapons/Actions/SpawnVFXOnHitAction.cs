using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class SpawnHitVFXOnAction : IWeaponAction
{
    public GameObject hitVFXPrefab;
    public LayerMask targetLayerMask = ~0;
    public float     hitRange      = 2.5f;
    public float     hitRadius = 0.5f;
    public float     vfxLifetime = 1.5f;
    public Vector3   vfxScale    = Vector3.one;

    public void Execute(WeaponContext ctx, InputBindingData b, ActionBindingData a)
    {
        var cam = ctx.PlayerCamera;
        Vector3 origin    = cam.transform.position;
        Vector3 direction = cam.transform.forward;
        
        RaycastHit[] hits = Physics.SphereCastAll(
                origin,
                hitRadius,
                direction,
                hitRange,
                targetLayerMask
            )
            .OrderBy(h => h.distance)   // closest first
            .ToArray();
        
        foreach (var h in hits)
        {
            // skip anything on own body
            if (h.collider.transform.IsChildOf(ctx.transform)) 
                continue;

            SpawnVFXAt(h.point, h.normal);
        }
    }

    private void SpawnVFXAt(Vector3 pos, Vector3 normal)
    {
        if (hitVFXPrefab == null) return;
        var vfx = GameObject.Instantiate(hitVFXPrefab, pos, Quaternion.LookRotation(normal));
        vfx.transform.localScale = vfxScale;
        GameObject.Destroy(vfx, vfxLifetime);
    }
}