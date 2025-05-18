using UnityEngine;
using UnityEngine.Rendering;

public class MeleeExecute : ExecuteComponent, IHoldHandler, IPressHandler, IReleaseHandler
{
    [Tooltip("All your melee settings")]
    public MeleeDataSO meleeData;

    public override void Initialize(WeaponContext ctx)
    {
        base.Initialize(ctx);
    }

    public void OnHold() { }
    
    public void OnPress() { MeleeHit(); }
    
    public void OnRelease() { }

    private void MeleeHit()
    {
        Collider[] hits = Physics.OverlapSphere(ctx.FirePoint.position, meleeData.range, meleeData.targetLayer, QueryTriggerInteraction.Ignore);
        foreach (var c in hits)
        {
            foreach (var hitComp in ctx.OnHitComponents)
            { 
                hitComp.OnHit(new CollisionInfo(c));
            }
        }
    }
}