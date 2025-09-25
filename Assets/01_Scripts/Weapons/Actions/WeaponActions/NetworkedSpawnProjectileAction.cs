using System;
using UnityEngine;
using FishNet.Object;

/// <summary>
/// Spawns a projectile at the fire points of the weapon
/// </summary>
[Serializable]
public class NetworkedSpawnProjectileAction : IWeaponAction
{
    [Tooltip("Projectile prefab to spawn")]
    public GameObject prefab;

    [Tooltip("Max distance for the aim ray if nothing is hit")]
    public float maxAimDistance = 1000f;
    
    public void Execute(WeaponContext ctx, InputBindingData binding, ActionBindingData actionBinding)
    {
        var networkObject = ctx.Player.GetComponent<NetworkObject>();
        if (networkObject != null && !networkObject.IsOwner) return;
        
        Camera cam = ctx.PlayerCamera != null ? ctx.PlayerCamera : Camera.main;
        Ray aimRay = cam.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        Vector3 aimPoint;

        // Raycast to find target point
        if (Physics.Raycast(aimRay, out RaycastHit hit, maxAimDistance))
        {
            aimPoint = hit.point;
        }
        else
        {
            aimPoint = aimRay.origin + aimRay.direction * maxAimDistance;
        }
        
        // Get the network spawner
        var spawner = ctx.Player.GetComponent<SimpleNetworkProjectileSpawner>();

        // Spawn a projectile from each fire point
        foreach (Transform fp in ctx.GetFirePointsFor(binding.hand))
        {
            Vector3 spawnPos = fp.position;
            Vector3 spawnDir = (aimPoint - spawnPos).normalized;
            Quaternion rot = Quaternion.LookRotation(spawnDir, fp.up);

            if (spawner != null)
            {
                var projectileController = prefab.GetComponent<ProjectileController>();
                float speed = projectileController != null ? projectileController.speed : 15f;
                spawner.SpawnNetworkedProjectile(prefab, spawnPos, rot, spawnDir * speed);
            }
            else
            {
                // Fallback to local spawning
                var go = GameObject.Instantiate(prefab, spawnPos, rot);
                if (go.TryGetComponent<ProjectileController>(out var pc))
                {
                    pc.Initialize(ctx.Player.gameObject, spawnDir * pc.speed);
                }
            }
        }
    }
}
