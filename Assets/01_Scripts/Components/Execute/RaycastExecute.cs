using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastExecute : ExecuteComponent, IHoldHandler, IPressHandler
{
    [Tooltip("All your raycast settings")]
    public RaycastDataSO raycastData;

    Camera _camera;

    public override void Initialize(WeaponContext ctx)
    {
        base.Initialize(ctx);
        _camera = ctx.PlayerCamera;
    }

    public void OnHold() { CastRay(); }
    public void OnPress() { CastRay(); }

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