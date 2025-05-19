using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProjectileExecute : ExecuteComponent
{
    [Tooltip("Projectile settings)")]
    public ProjectileDataSO projectileData;
    public FireTrigger fireTrigger = FireTrigger.OnPress;

    public float fireRate = 0.1f;
    private float _nextFireTime;
    
    Camera _playerCamera;

    public override void Initialize(WeaponContext ctx)
    {
        base.Initialize(ctx);
        _playerCamera = ctx.PlayerCamera;
    }

    public override void OnHold()
    {
        if (!fireTrigger.HasFlag(FireTrigger.OnHold)) return;
        if (Time.time < _nextFireTime) return;
        _nextFireTime = Time.time + fireRate;
        
        SpawnProjectile();
    }

    public override void OnPress()
    {
        if (!fireTrigger.HasFlag(FireTrigger.OnPress)) return;
        SpawnProjectile();
    }

    public override void OnRelease()
    {
        if (!fireTrigger.HasFlag(FireTrigger.OnRelease)) return;
        SpawnProjectile();
    }
    
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