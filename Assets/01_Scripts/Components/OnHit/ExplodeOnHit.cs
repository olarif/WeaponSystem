using UnityEngine;

public class ExplodeOnHit : OnHitComponent
{
    public AoEDataSO aoeData;
    
    public override void Initialize(WeaponContext context)
    {
        Debug.Log("ExplodeOnHit initialized! ");
    }
    
    public override void OnHit(CollisionInfo info)
    {
        Debug.Log("ExplodeOnHit triggered! ");
    }
}