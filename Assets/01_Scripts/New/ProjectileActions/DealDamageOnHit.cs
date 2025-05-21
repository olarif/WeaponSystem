using UnityEngine;

[System.Serializable]
public class DealDamageOnHit : ProjectileActionData
{
    public DamageType damageType;
    public float damage = 5;
    
    public override void Execute(GameObject projectile, CollisionInfo collision, GameObject owner)
    {
        Debug.Log("DealDamageOnHit Projectile triggered");
        
        if (collision.HitObject.TryGetComponent<IDamageable>(out var d))
            d.TakeDamage(damage, damageType);
    }
}