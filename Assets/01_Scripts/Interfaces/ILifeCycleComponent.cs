public interface ILifeCycleComponent
{
    void Initialize(WeaponContext context);
    void OnStart();
    void OnUpdate();
    void OnStop();
    void Cleanup();
}