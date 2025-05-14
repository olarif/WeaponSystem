using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class RaycastExecute : ExecuteComponent
{
    public RaycastDataSO raycastData;
    
    private LineRenderer _lineRenderer;
    
    public override void Initialize(WeaponContext context)
    {
         WeaponContext = context;
         _lineRenderer = context.LineRenderer;
    }

    public override void Execute()
    {
        Ray ray = new Ray(WeaponContext.FirePoint.position, WeaponContext.FirePoint.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastData.range, raycastData.hitLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.ApplyDamage(raycastData.damage);
                
                foreach (var hitComp in WeaponContext.OnHitComponents)
                {
                    hitComp.OnHit(new CollisionInfo(hit));
                }
            }
        }
        Linecast();
    }
    
    public override void CancelExecute()
    {
        _lineRenderer.enabled = false;
    }

    private void Linecast()
    {
        _lineRenderer.enabled = true;
        
        _lineRenderer.positionCount = 2;
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;
        var targetPosition = WeaponContext.FirePoint.position;
            
        _lineRenderer.SetPosition(0, targetPosition);
        _lineRenderer.SetPosition(1, targetPosition + (WeaponContext.FirePoint.forward * raycastData.range));
    }
}