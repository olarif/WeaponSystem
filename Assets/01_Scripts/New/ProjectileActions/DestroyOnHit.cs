using UnityEngine;

public class DestroyOnHit : ProjectileActionData
{
    public override void Execute(GameObject projectile, CollisionInfo collision, GameObject owner)
    {
        // Destroy the projectile on hit
        Object.Destroy(projectile);
    }
}