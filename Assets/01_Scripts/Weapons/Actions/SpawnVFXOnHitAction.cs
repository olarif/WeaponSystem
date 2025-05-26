using System;
using UnityEngine;

[Serializable]
public class SpawnHitVFXOnAction : IWeaponAction
{
    public GameObject hitVFXPrefab;
    public LayerMask targetLayerMask = ~0;
    public float     hitRange      = 2.5f;
    public float     vfxLifetime = 1.5f;
    public Vector3   vfxScale    = Vector3.one;

    public void Execute(WeaponContext ctx, InputBindingData b, ActionBindingData a)
    {
        Vector3 origin = ctx.transform.position;
        var hits = Physics.OverlapSphere(origin, hitRange, targetLayerMask);

        foreach (var c in hits)
        {
            if (c.transform.IsChildOf(ctx.transform)) continue;

            // compute exact hit point & normal
            Vector3 dir = (c.transform.position - origin).normalized;
            if (c.Raycast(new Ray(origin, dir), out var hit, Mathf.Infinity))
            {
                SpawnVFXAt(hit.point, hit.normal);
            }
            else
            {
                var pt = c.ClosestPoint(origin);
                var n  = (pt - origin).normalized;
                SpawnVFXAt(pt, n);
            }
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