public abstract class OnHitComponent : WeaponComponent, IOnHitComponent
{
    public override void Initialize(WeaponContext context) { }
    public abstract void OnHit(CollisionInfo info);
}