using System.Collections.Generic;
using UnityEngine;

public class VFXOnHit : OnHitComponent
{
    public List<GameObject> vfxEffects;
    public float duration = 2f;
    public override void Initialize(WeaponContext ctx)
    {
        base.Initialize(ctx);
    }
    
    public override void OnHit(CollisionInfo info)
    {
        foreach (GameObject vfx in vfxEffects)
        {
            var fx = Instantiate(vfx, info.Point, Quaternion.identity);
            Destroy(fx, duration);
        }
    }
}