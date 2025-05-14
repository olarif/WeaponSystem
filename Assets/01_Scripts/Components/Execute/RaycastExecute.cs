using UnityEngine;

public class RaycastExecute : ExecuteComponent
{
    public RaycastDataSO raycastData;
    
    public override void Initialize(WeaponContext context)
    {
         WeaponContext = context;
    }
    
    public override void Execute()
    {
        if (raycastData == null)
        {
            Debug.LogError("RaycastData is not assigned.");
            return;
        }
        
        Ray ray = new Ray(WeaponContext.FirePoint.position, WeaponContext.FirePoint.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastData.range, raycastData.collisionMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Hit detected: " + hit.collider.name);
            
            if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.ApplyDamage(raycastData.damage);
            }
            
            foreach (var hitComp in WeaponContext.OnHitComponents)
            {
                hitComp.OnHit(new CollisionInfo(hit));
            }
        }
        else
        {
            Debug.Log("No hit detected.");
        }
        
        Debug.DrawRay(ray.origin, ray.direction * raycastData.range, Color.red, 1f, false);
    }
}