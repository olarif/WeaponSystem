public interface IInputComponent
{
    void Initialize(WeaponContext context);
    bool CanExecute();
    bool IsExecuting();
}