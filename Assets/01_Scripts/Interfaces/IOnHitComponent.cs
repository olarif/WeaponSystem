
public interface IOnHitComponent
{
        void Initialize(WeaponContext context);
        void OnHit(CollisionInfo info);
}