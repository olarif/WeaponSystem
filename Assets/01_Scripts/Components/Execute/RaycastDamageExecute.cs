using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastDamageExecute : ExecuteComponent
{
    [Tooltip("All your raycast settings")]
    public RaycastDataSO raycastData;
    [Tooltip("How long the ray will be visible")]

    Camera _camera;

    public override void Initialize(WeaponContext ctx)
    {
        base.Initialize(ctx);
        _camera = ctx.PlayerCamera;
    }

    public override void OnStart()   => CastRay();
    public override void OnUpdate()  => CastRay();
    public override void OnStop() {}

    public override void Execute() { }

    void CastRay()
    {
        var center = new Vector2(Screen.width/2f, Screen.height/2f);
        var ray    = _camera.ScreenPointToRay(center);

        if (Physics.Raycast(ray, out var hit, raycastData.range, raycastData.hitLayers))
        {
            hit.collider.GetComponent<IDamageable>()?.ApplyDamage(raycastData.damage);
            foreach (var h in ctx.OnHitComponents)
                h.OnHit(new CollisionInfo(hit));
        }
    }
}