using UnityEngine;

[System.Serializable]
public class StickOnHit : ProjectileActionData
{
    public override void Execute(GameObject projectile, CollisionInfo collision, GameObject owner)
    {

        projectile.transform.SetParent(collision.HitObject.transform);
        
        //disable the projectile collider
        if (projectile.TryGetComponent<Collider>(out var col))
            col.enabled = false;    
        
        //disable the projectile rigidbody
        if (projectile.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;
    }
}