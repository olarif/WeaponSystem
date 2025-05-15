using UnityEngine;

public abstract class WeaponComponent : ScriptableObject, ILifeCycleComponent
{ 
    protected WeaponContext ctx;

    public virtual void Initialize(WeaponContext context)
    {
        ctx = context;
    }
    
    public virtual void OnStart() { }
    public virtual void OnUpdate() { }
    public virtual void OnStop() { }
    public virtual void Cleanup() { }
}