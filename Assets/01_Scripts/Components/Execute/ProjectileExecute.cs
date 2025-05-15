using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProjectileExecute : ExecuteComponent
{
    public ProjectileDataSO projectileData;

    public override void Execute()
    {
        Debug.Log( $"Executing {GetType().Name} for {projectileData.name} with projectile data: {projectileData}" );
        
        GameObject projectile = Instantiate(projectileData.projectilePrefab, ctx.FirePoint.position, ctx.FirePoint.rotation);
        projectile.GetComponent<Projectile>().Initialize(projectileData, ctx, ctx.FirePoint.forward);
        
        iTween.PunchPosition(projectile, new Vector3 (Random.Range(-projectileData.arcRange, projectileData.arcRange), Random.Range(-projectileData.arcRange, projectileData.arcRange), 0), projectileData.lifetime);
    }
}