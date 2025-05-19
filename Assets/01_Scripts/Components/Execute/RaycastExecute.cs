using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class RaycastExecute : ExecuteComponent
{
    [Tooltip("All your raycast settings")]
    public RaycastDataSO raycastData;
    public FireTrigger fireTrigger = FireTrigger.OnPress;
    
    public float fireRate = 0.1f;
    float _timeSinceLastShot;

    Camera _camera;

    public override void Initialize(WeaponContext ctx)
    {
        base.Initialize(ctx);
        _camera = ctx.PlayerCamera;
        _timeSinceLastShot = 0f;
    }

    public override void OnHold()
    {
        if (!fireTrigger.HasFlag(FireTrigger.OnHold)) return;
        _timeSinceLastShot += Time.deltaTime;
        
        if (_timeSinceLastShot < fireRate) return;
        _timeSinceLastShot = 0f;
        CastRay();
    }

    public override void OnPress()
    {
        if (!fireTrigger.HasFlag(FireTrigger.OnPress)) return;
        CastRay(); 
        _timeSinceLastShot = 0f;
    }

    public override void OnRelease()
    {
        if (!fireTrigger.HasFlag(FireTrigger.OnRelease)) return;
        CastRay() ;
        _timeSinceLastShot = 0f;
    }

    void CastRay()
    {
        var center = new Vector2(Screen.width/2f, Screen.height/2f);
        var ray    = _camera.ScreenPointToRay(center);

        if (Physics.Raycast(ray, out var hit, raycastData.range, raycastData.hitLayers))
        {
            // Dispatch to all OnHitComponents
            foreach (var h in ctx.OnHitComponents)
                h.OnHit(new CollisionInfo(hit));
                
        }
    }
}