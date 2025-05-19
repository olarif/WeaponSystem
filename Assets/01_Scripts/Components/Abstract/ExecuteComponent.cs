public abstract class ExecuteComponent : WeaponComponent, IPressHandler, IHoldHandler, IReleaseHandler
{
    public virtual void Tick(float deltaTime) { }

    public virtual void OnHold() { }

    public virtual void OnRelease() { }

    public virtual void OnPress() { }
}