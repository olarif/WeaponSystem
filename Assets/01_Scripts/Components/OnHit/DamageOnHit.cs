using UnityEngine;

public class DamageOnHit : OnHitComponent
{
    public float damage;
    
    public override void Initialize(WeaponContext ctx)
    {
        base.Initialize(ctx);
    }
    
    public override void OnHit(CollisionInfo info)
    {
        Debug.Log("Hit detected!");
        // Try to deal damage
        var target = info.HitObject.GetComponent<IDamageable>();
        
        if (target != null) 
            target.ApplyDamage(damage);
    }
}