using UnityEngine;

public abstract class WeaponComponent : ScriptableObject
{ 
    protected WeaponContext ctx;

    public virtual void Initialize(WeaponContext context)
    {
        ctx = context;
    }

    public virtual void Cleanup() { }
}