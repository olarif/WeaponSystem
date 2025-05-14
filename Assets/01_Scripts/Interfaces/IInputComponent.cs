public interface IInputComponent
{
    void Initialize(WeaponContext context);
    bool CanExecute();
}