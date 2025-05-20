using UnityEngine;

public class SpawnProjectileAction : WeaponActionData
{
    public GameObject projectilePrefab;
    public bool useLeftHand = false;
    public bool toggleHands = false;
    
    public override void Execute(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        if (projectilePrefab == null) return;
        
        var hand = useLeftHand ? ctx.leftHand : ctx.rightHand;
        
        // toggle between hands
        if (toggleHands)
        {
            useLeftHand = !useLeftHand;
            hand = useLeftHand ? ctx.leftHand : ctx.rightHand;
        }
        
        Vector3 pos = hand.position;
        Quaternion rot = hand.rotation;
        // Instantiate
        var projectile = Object.Instantiate(projectilePrefab, pos, rot);
        // Initialize with owner
        var controller = projectile.GetComponent<ProjectileController>();
        if (controller != null)
            controller.Initialize(ctx.gameObject);
    }
}
