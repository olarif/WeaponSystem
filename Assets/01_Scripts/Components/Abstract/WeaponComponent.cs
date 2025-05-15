using UnityEngine;

public abstract class WeaponComponent : ScriptableObject
{ 
    protected WeaponContext WeaponContext;
    public abstract void Initialize(WeaponContext context);
}