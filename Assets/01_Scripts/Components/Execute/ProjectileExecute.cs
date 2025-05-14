using UnityEngine;

public class ProjectileExecute : ExecuteComponent
{
    public ProjectileDataSO projectileData;

    public override void Initialize(WeaponContext context)
    {
        WeaponContext = context;
    }
    
    public override void Execute()
    {
        if (projectileData == null)
        {
            Debug.LogError("Projectile data is not set.");
            return;
        }
        
        if (projectileData.projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab is not set.");
            return;
        }
        
        Debug.Log("Executing projectile attack with data: " + projectileData.name);

        
        
        GameObject projectile = Instantiate(projectileData.projectilePrefab, WeaponContext.FirePoint.position, WeaponContext.FirePoint.rotation);
        
        projectile.GetComponent<Projectile>().Initialize(projectileData, WeaponContext, WeaponContext.FirePoint.forward);
    }
}