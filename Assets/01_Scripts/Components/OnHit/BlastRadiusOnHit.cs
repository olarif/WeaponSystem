using UnityEngine;

public class BlastRadiusOnHit : OnHitComponent
{
    public AoEDataSO aoeData;
    
    public override void Initialize(WeaponContext ctx)
    {
        base.Initialize(ctx);
    }
    
    public override void OnHit(CollisionInfo info)
    {
        /*Collider[] hits = Physics.OverlapSphere(info.Point, aoeData.radius, aoeData.targetLayer, QueryTriggerInteraction.Ignore);
        foreach (var c in hits)
        {
            foreach (var hitComp in ctx.OnHitComponents)
            { 
                hitComp.OnHit(new CollisionInfo(c));
            }
        }*/
    }
}