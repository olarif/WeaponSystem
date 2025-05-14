public abstract class ExecuteComponent : WeaponComponent, IExecuteComponent
{
    public override void Initialize(WeaponContext context) { }
    public abstract void Execute();
    public abstract void CancelExecute();
    
}