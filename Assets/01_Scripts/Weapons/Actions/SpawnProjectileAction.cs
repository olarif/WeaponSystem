using System;
using UnityEngine;

[Serializable]
public class SpawnProjectileAction : IWeaponAction
{
    [Tooltip("Projectile prefab to spawn")]
    public GameObject prefab;

    [Tooltip("Max distance for the aim‐ray if nothing is hit")]
    public float maxAimDistance = 1000f;

    public void Execute(WeaponContext ctx, InputBindingData b, ActionBindingData ab)
    {
        Camera cam = ctx.PlayerCamera != null ? ctx.PlayerCamera : Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("SpawnProjectileAction: no Camera found on WeaponContext or MainCamera.");
        }
        
        Ray aimRay = cam != null
            ? cam.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f))
            : default;
        
        Vector3 aimPoint;
        if (cam != null && Physics.Raycast(aimRay, out RaycastHit hit, maxAimDistance))
        {
            aimPoint = hit.point;
        }
        else
        {
            aimPoint = (cam != null)
                ? aimRay.origin + aimRay.direction * maxAimDistance
                : Vector3.zero;
        }
        
        foreach (var fp in ctx.GetFirePointsFor(b.hand))
        {
            Vector3 spawnPos = fp.position;
            Vector3 aimDir   = (aimPoint - spawnPos).normalized;
            Quaternion rot   = Quaternion.LookRotation(aimDir, fp.up);

            var proj = UnityEngine.Object.Instantiate(prefab, spawnPos, rot);
            if (proj.TryGetComponent<ProjectileController>(out var pc))
            {
                pc.Initialize(ctx.WeaponController.gameObject);
            }
        }
    }
}