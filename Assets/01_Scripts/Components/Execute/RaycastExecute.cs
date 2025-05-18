using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class RaycastExecute : ExecuteComponent, IHoldHandler, IPressHandler, IReleaseHandler
{
    [Tooltip("All your raycast settings")]
    public RaycastDataSO raycastData;
    
    public float fireRate = 0.1f;
    float _timeSinceLastShot;

    Camera _camera;

    public override void Initialize(WeaponContext ctx)
    {
        base.Initialize(ctx);
        _camera = ctx.PlayerCamera;
        _timeSinceLastShot = 0f;
    }

    public void OnHold()
    {
        _timeSinceLastShot += Time.deltaTime;
        
        if (_timeSinceLastShot < fireRate) return;
        _timeSinceLastShot = 0f;
        CastRay();
    }
    
    public void OnPress() { CastRay(); _timeSinceLastShot = 0f; }
    
    public void OnRelease() { CastRay() ;_timeSinceLastShot = 0f; }

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