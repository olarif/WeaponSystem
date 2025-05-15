using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProjectileExecute : ExecuteComponent
{
    public ProjectileDataSO projectileData;
    
    private Camera _playerCamera;
    
    public override void Initialize(WeaponContext context)
    {
        base.Initialize(context);
        _playerCamera = context.PlayerCamera;
    }

    public override void Execute()
    {
        // 1) Cast a ray from the center of the screen
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = _playerCamera.ScreenPointToRay(screenCenter);

        // 2) Decide spawn rotation so projectile faces the ray
        Quaternion spawnRot = Quaternion.LookRotation(ray.direction, Vector3.up);

        // 3) Instantiate at your fire point
        GameObject projGO = Instantiate(
            projectileData.projectilePrefab, 
            ctx.FirePoint.position, 
            spawnRot
        );

        // 4) Initialize its logic with the exact ray direction
        projGO.GetComponent<Projectile>()
            .Initialize(projectileData, ctx, ray.direction);

        // 5) Add a little random arc punch if desired
        Vector3 arc = new Vector3(
            Random.Range(-projectileData.arcRange, projectileData.arcRange),
            Random.Range(-projectileData.arcRange, projectileData.arcRange),
            0
        );
        iTween.PunchPosition(projGO, arc, projectileData.lifetime);
    }
}