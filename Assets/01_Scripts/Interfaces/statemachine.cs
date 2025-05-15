using UnityEngine;

public interface ILifeCycleComponent
{
    void Initialize(WeaponContext context);
    void OnStart();
    void OnUpdate();
    void OnFixedUpdate();
    void OnLateUpdate();
    void OnStop();
    void OnCleanup();
}