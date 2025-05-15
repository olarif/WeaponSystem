using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastExecute : ExecuteComponent
{
    [Tooltip("All your raycast settings")]
    public RaycastDataSO raycastData;
    [Tooltip("How long the ray will be visible")]
    
    LineRenderer _lr;
    Camera _camera;

    public override void Initialize(WeaponContext ctx)
    {
        base.Initialize(ctx);
        _lr     = ctx.LineRenderer;
        _camera = ctx.PlayerCamera;
        if (_lr != null)
        {
            // we need two points!
            _lr.positionCount = 2;
            _lr.enabled       = false;
        }
    }

    public override void OnStart()   => DrawRay();
    public override void OnUpdate()  => DrawRay();
    public override void OnStop() { _lr.enabled = false; }

    public override void Cleanup()
    {
        if (_lr != null) _lr.enabled = false;
    }
    
    public override void Execute() { }

    void DrawRay()
    {
        var origin = ctx.FirePoint.position;
        var center = new Vector2(Screen.width/2f, Screen.height/2f);
        var ray    = _camera.ScreenPointToRay(center);

        if (Physics.Raycast(ray, out var hit, raycastData.range, raycastData.hitLayers))
        {
            hit.collider.GetComponent<IDamageable>()?.ApplyDamage(raycastData.damage);
            foreach (var h in ctx.OnHitComponents)
                h.OnHit(new CollisionInfo(hit));
            UpdateLine(origin, hit.point);
        }
        else
        {
            UpdateLine(origin, ray.origin + ray.direction * raycastData.range);
        }
    }

    void UpdateLine(Vector3 start, Vector3 end)
    {
        if (_lr == null) return;
        if (!_lr.enabled) _lr.enabled = true;
        _lr.SetPosition(0, start);
        _lr.SetPosition(1, end);
    }
    
}