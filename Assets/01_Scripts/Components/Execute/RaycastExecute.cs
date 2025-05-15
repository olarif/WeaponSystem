using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastExecute : ExecuteComponent
{
    public RaycastDataSO raycastData;

    public override void Execute()
    {
        Vector2 screenPos = Mouse.current?.position.ReadValue() ?? Vector2.zero;
        Ray ray = ctx.PlayerCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastData.range, raycastData.hitLayers))
        {
            Debug.Log("Hit detected: " + hit.collider.name);
            hit.collider.GetComponent<IDamageable>()?.ApplyDamage(raycastData.damage);

            foreach (var hitComp in ctx.OnHitComponents)
            {
                hitComp.OnHit(new CollisionInfo(hit));
            }
        }
        else
        {
            Debug.Log("No hit detected.");
        }
    }
    
}