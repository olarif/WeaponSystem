using UnityEngine;

[System.Serializable]
public abstract class ProjectileActionData
{
    public abstract void Execute(GameObject projectile, CollisionInfo collision, GameObject owner);
}