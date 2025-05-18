using UnityEngine;

public class LineRenderExecute : ExecuteComponent, IHoldHandler, IPressHandler, IReleaseHandler
{
    [Tooltip("Projectile data for the line renderer")]
    public LineRendererDataSO lrData;
    
    //how long to keep ray active after cast
    public float rayDuration = 0.1f;
    
    LineRenderer _lr;
    Camera _camera;
    
    private float _timeSinceLastRay;
    private bool _isHolding;

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

        _timeSinceLastRay = rayDuration;
    }

    public void OnPress() { _isHolding = true; DrawRay(); }
    public void OnHold()  { _isHolding = true; DrawRay(); }

    public void OnRelease()
    {
        _isHolding = false;
        if (rayDuration <= 0f)
        {
            DisableRay();
        }
        else
        {
            DrawRay();
            _timeSinceLastRay = 0f;
        }
    }
    
    public override void Tick(float dt)
    {
        if (!_isHolding && rayDuration > 0f)
        {
            _timeSinceLastRay += dt;
            if (_timeSinceLastRay >= rayDuration)
            {
                DisableRay();
            }
        }
    }
    
    public override void Cleanup() { DisableRay(); }

    void DisableRay()
    {
        if (_lr != null) _lr.enabled = false;
    }

    void DrawRay()
    {
        _timeSinceLastRay = 0f;
        
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