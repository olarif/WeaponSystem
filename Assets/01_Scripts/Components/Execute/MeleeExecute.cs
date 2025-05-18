using UnityEngine;
using UnityEngine.Rendering;

public class MeleeExecute : ExecuteComponent, IHoldHandler, IPressHandler, IReleaseHandler
{
    [Tooltip("All your melee settings")]
    public MeleeDataSO meleeData;
    
    public FireTrigger fireTrigger = FireTrigger.OnPress;

    public override void Initialize(WeaponContext ctx)
    {
        base.Initialize(ctx);
    }

    public void OnHold()
    {
        if (!fireTrigger.HasFlag(FireTrigger.OnHold)) return;
        MeleeHit();
    }

    public void OnPress()
    {
        if (!fireTrigger.HasFlag(FireTrigger.OnPress)) return;
        MeleeHit();
    }

    public void OnRelease()
    {
        if (!fireTrigger.HasFlag(FireTrigger.OnRelease)) return;
        MeleeHit();
    }

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