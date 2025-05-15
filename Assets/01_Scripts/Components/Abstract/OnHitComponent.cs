public abstract class OnHitComponent : WeaponComponent, IOnHitComponent
{
    public abstract void OnHit(CollisionInfo info);
}