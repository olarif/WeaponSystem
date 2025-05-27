using UnityEngine;

[System.Serializable]
public abstract class ProjectileComponent
{
    [SerializeField] protected bool isEnabled = true;
    
    public bool IsEnabled => isEnabled;
    
    // Called once when projectile is created
    public virtual void Initialize(ProjectileRuntimeData data) { }
    
    // Called every frame
    public virtual void UpdateComponent(ProjectileRuntimeData data) { }
    
    // Called every physics update
    public virtual void FixedUpdateComponent(ProjectileRuntimeData data) { }
    
    // Called when projectile collides with something
    public virtual ComponentResult OnCollision(ProjectileRuntimeData data, CollisionInfo collision) 
    { 
        return ComponentResult.Continue; 
    }
    
    // Called when projectile is destroyed
    public virtual void OnDestroy(ProjectileRuntimeData data) { }
}

public enum ComponentResult
{
    Continue,
    DestroyProjectile,
    Stop
}