using UnityEngine;
using UnityEngine.Rendering;

public class MeleeExecute : ExecuteComponent
{
    public MeleeDataSO meleeData;

    public override void Execute()
    { 
        Debug.Log("Melee attack");
        
        Collider[] hits = Physics.OverlapSphere(ctx.FirePoint.position, meleeData.range, meleeData.targetLayer, QueryTriggerInteraction.Ignore);
        foreach (var c in hits)
        { 
            //Debug.Log("Hit detected: " + c.name);
            c.GetComponent<IDamageable>()?.ApplyDamage(meleeData.damage);
           
            foreach (var hitComp in ctx.OnHitComponents)
            { 
                hitComp.OnHit(new CollisionInfo(c));
            }
        } 
    }
}