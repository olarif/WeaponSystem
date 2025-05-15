public abstract class ExecuteComponent : WeaponComponent, IExecuteComponent
{
    public override void OnStart()
    {
        Execute();
    }
    
    public abstract void Execute();
}