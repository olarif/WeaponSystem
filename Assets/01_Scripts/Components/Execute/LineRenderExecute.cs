using UnityEngine;

public class LineRenderExecute : ExecuteComponent, IHoldHandler, IPressHandler, IReleaseHandler
{
    [Tooltip("Projectile data for the line renderer")]
    public LineRendererDataSO lrData;
    
    LineRenderer _lr;
    Camera _camera;

    public override void Initialize(WeaponContext ctx)
    {
        base.Initialize(ctx);
        _lr     = ctx.LineRenderer;
        _camera = ctx.PlayerCamera;
        
        if (_lr != null)
        {
            _lr.positionCount = 2;
            _lr.enabled       = false;
            _lr.startWidth    = lrData.lineWidth;
            _lr.endWidth      = lrData.lineWidth;
        }
    }

    public void OnHold() { DrawRay(); }
    
    public void OnPress() { DrawRay(); }
    
    public void OnRelease(){ DisableRay(); }
    
    public override void Cleanup() { DisableRay(); }

    void DisableRay()
    {
        if (_lr != null) _lr.enabled = false;
    }

    void DrawRay()
    {
        var origin = ctx.FirePoint.position;
        var center = new Vector2(Screen.width/2f, Screen.height/2f);
        var ray    = _camera.ScreenPointToRay(center);

        if (Physics.Raycast(ray, out var hit, lrData.range))
        {
            UpdateLine(origin, hit.point);
        }
        else
        {
            UpdateLine(origin, ray.origin + ray.direction * lrData.range);
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