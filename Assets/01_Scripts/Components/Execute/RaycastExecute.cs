using UnityEngine;

public class RaycastExecute : ExecuteComponent
{
    public RaycastDataSO raycastData;
    public bool isLinecast;
    
    private LineRenderer _lineRenderer;
    private Transform _firePoint;
    
    public override void Initialize(WeaponContext context)
    {
         WeaponContext = context;
         _lineRenderer = context.LineRenderer;
         
         _firePoint = context.FirePoint;
         _lineRenderer.enabled = false;
    }

    public override void Execute()
    {
        Linecast();
        
        Ray ray = new Ray(WeaponContext.FirePoint.position, WeaponContext.FirePoint.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastData.range, raycastData.hitLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.ApplyDamage(raycastData.damage);
            }
            
            foreach (var hitComp in WeaponContext.OnHitComponents)
            {
                hitComp.OnHit(new CollisionInfo(hit));
            }
        }
    }

    private void Linecast()
    {
        if (!isLinecast) return;
        
        _lineRenderer.enabled = true;
        
        _lineRenderer.positionCount = 2;
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;

        var position = _firePoint.position;
        _lineRenderer.SetPosition(0, position);
        _lineRenderer.SetPosition(1, position + (WeaponContext.FirePoint.forward * raycastData.range));
    }
}