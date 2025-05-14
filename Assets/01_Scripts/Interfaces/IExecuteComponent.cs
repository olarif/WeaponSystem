public interface IExecuteComponent
{
    void Initialize(WeaponContext context);
    void Execute();
    void CancelExecute();
}