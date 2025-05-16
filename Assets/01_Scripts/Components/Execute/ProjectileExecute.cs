using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProjectileExecute : ExecuteComponent, IPressHandler, IHoldHandler, IReleaseHandler
{
    [Tooltip("Projectile settings)")]
    public ProjectileDataSO projectileData;

    public float fireRate = 0.1f;
    private float _nextFireTime;
    
    Camera _playerCamera;

    public override void Initialize(WeaponContext ctx)
    {
        base.Initialize(ctx);
        _playerCamera = ctx.PlayerCamera;
    }

    public void OnHold()
    {
        if (Time.time < _nextFireTime) return;
        _nextFireTime = Time.time + fireRate;
        
        SpawnProjectile();
    }
    
    public void OnPress(){ SpawnProjectile(); }
    
    public void OnRelease() { SpawnProjectile();}
    
    private void SpawnProjectile()
    {
        var screenCenter = new Vector2(Screen.width/2f, Screen.height/2f);
        var ray          = _playerCamera.ScreenPointToRay(screenCenter);
        var spawnRot     = Quaternion.LookRotation(ray.direction, Vector3.up);

        // offset forward a bit so you don't intersect yourself
        var spawnPos = ctx.FirePoint.position + ray.direction * 0.5f;

        GameObject go = Instantiate(
            projectileData.projectilePrefab,
            spawnPos,
            spawnRot
        );

        go.GetComponent<Projectile>()
            .Initialize(projectileData, ctx, ray.direction);

        // optional arc punch
        var arc = new Vector3(
            Random.Range(-projectileData.arcRange, projectileData.arcRange),
            Random.Range(-projectileData.arcRange, projectileData.arcRange),
            0
        );
        iTween.PunchPosition(go, arc, projectileData.lifetime);
    }
}