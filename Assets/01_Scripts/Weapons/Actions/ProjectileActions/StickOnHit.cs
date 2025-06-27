using UnityEngine;

[System.Serializable]
public class StickOnHit : ProjectileComponent
{
    [Tooltip("Should the projectile stick to the target on collision?")]
    public bool stickOnCollision = true;

    public override ComponentResult OnCollision(ProjectileRuntimeData data, CollisionInfo ci)
    {
        if (stickOnCollision)
        {
            // Stick the projectile to the target
            data.projectile.transform.SetParent(ci.HitObject.transform, true);
            data.projectile.transform.position = ci.Point;
            data.projectile.GetComponent<Rigidbody>().isKinematic = true;
        }
        return ComponentResult.Continue;
    }
}