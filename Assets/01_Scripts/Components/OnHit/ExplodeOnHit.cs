using UnityEngine;

public class ExplodeOnHit : OnHitComponent
{
    public AoEDataSO aoeData;
    
    public override void OnHit(CollisionInfo info)
    {
        GameObject explosion = Instantiate(aoeData.explosionPrefab, info.Point, Quaternion.identity);
        
        //explosion
    }
}