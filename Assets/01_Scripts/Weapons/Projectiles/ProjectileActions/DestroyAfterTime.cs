using UnityEngine;

[System.Serializable]
public class DestroyAfterTime : ProjectileActionData
{
    public float timeToDestroy = 3f;
    
    public override void Execute(GameObject projectile, CollisionInfo collision, GameObject owner)
    {
        Object.Destroy(projectile, timeToDestroy);
    }
}