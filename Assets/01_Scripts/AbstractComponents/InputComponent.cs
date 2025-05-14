public abstract class InputComponent : WeaponComponent, IInputComponent
{
    public override void Initialize(WeaponContext context){ }
    public abstract bool CanExecute();
    public abstract bool IsExecuting();
}