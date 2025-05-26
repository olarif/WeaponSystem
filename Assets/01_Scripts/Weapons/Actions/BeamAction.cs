using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and manages beam behavior based on the selected mode (Press, Charge, Continuous).
/// Implements IWeaponAction to integrate with the weapon system.
/// </summary>
[Serializable]
public class EnhancedBeamAction : IWeaponAction
{
    public enum Mode { Press, Charge, Continuous }

    [Header("General Settings")]
    public Mode mode = Mode.Press;
    
    [Tooltip("VFX prefab with VisualEffect component for the beam")]
    public GameObject beamVFXPrefab;
    
    [Header("Targeting")]
    [Tooltip("Max distance to raycast for screen center targeting")]
    public float maxTargetDistance = 1000f;
    
    [Tooltip("Layers to consider for targeting")]
    public LayerMask targetingMask = ~0;

    [Header("Timing")]
    public float upTime = 0.2f;
    public float extraTime = 0.1f;

    [Header("Damage")]
    public float damageAmount = 10f;
    public float continuousDPS = 5f;
    public LayerMask damageMask = ~0;

    [Header("Beam Parameters")]
    [Tooltip("Maximum beam distance if no target found")]
    public float maxBeamDistance = 50f;

    /// <summary>
    /// Tracks active beam data for each input binding
    /// </summary>
    private readonly Dictionary<InputBindingData, BeamInstance> _activeBeams 
        = new Dictionary<InputBindingData, BeamInstance>();

    public void Execute(WeaponContext ctx, InputBindingData ib, ActionBindingData ab)
    {
        if (beamVFXPrefab == null) return;

        var firePoint = ctx.GetFirePointsFor(ib.hand).FirstOrDefault();
        if (firePoint == null) return;

        switch (mode)
        {
            case Mode.Press:
            case Mode.Charge:
                if (ab.triggerPhase != TriggerPhase.OnPerform) return;
                
                var targetInfo = CalculateBeamTarget(ctx, firePoint);
                var beam = SpawnBeam(firePoint, targetInfo, upTime, 0f);
                ApplyInstantDamage(firePoint, targetInfo);
                break;

            case Mode.Continuous:
                if (ab.triggerPhase == TriggerPhase.OnStart)
                {
                    var contTargetInfo = CalculateBeamTarget(ctx, firePoint);
                    var contBeam = SpawnBeam(firePoint, contTargetInfo, Mathf.Infinity, continuousDPS);
                    _activeBeams[ib] = contBeam;
                }
                else if (ab.triggerPhase == TriggerPhase.OnCancel)
                {
                    if (_activeBeams.TryGetValue(ib, out var activeBeam))
                    {
                        activeBeam.EndBeam(extraTime);
                        _activeBeams.Remove(ib);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Calculates the target point by raycasting from screen center
    /// </summary>
    private BeamTargetInfo CalculateBeamTarget(WeaponContext ctx, Transform firePoint)
    {
        Camera cam = ctx.PlayerCamera ?? Camera.main;
        if (cam == null)
        {
            // Fallback: shoot forward from firepoint
            return new BeamTargetInfo
            {
                targetPoint = firePoint.position + firePoint.forward * maxBeamDistance,
                hitSomething = false,
                distance = maxBeamDistance
            };
        }

        // Cast ray from screen center
        Ray screenRay = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
        
        Vector3 targetPoint;
        bool hitSomething = false;
        float distance;

        if (Physics.Raycast(screenRay, out RaycastHit hit, maxTargetDistance, targetingMask))
        {
            targetPoint = hit.point;
            hitSomething = true;
            distance = Vector3.Distance(firePoint.position, hit.point);
        }
        else
        {
            targetPoint = screenRay.origin + screenRay.direction * maxTargetDistance;
            hitSomething = false;
            distance = Vector3.Distance(firePoint.position, targetPoint);
        }

        // Clamp distance to max beam distance
        if (distance > maxBeamDistance)
        {
            Vector3 direction = (targetPoint - firePoint.position).normalized;
            targetPoint = firePoint.position + direction * maxBeamDistance;
            distance = maxBeamDistance;
            hitSomething = false; // We're not actually hitting the original target
        }

        return new BeamTargetInfo
        {
            targetPoint = targetPoint,
            hitSomething = hitSomething,
            distance = distance
        };
    }

    /// <summary>
    /// Spawns and configures a VFX beam
    /// </summary>
    private BeamInstance SpawnBeam(Transform origin, BeamTargetInfo targetInfo, float lifeTime, float dps)
    {
        var go = GameObject.Instantiate(beamVFXPrefab, origin.position, origin.rotation);
        var beamController = go.GetComponent<BeamController>();
        
        if (beamController == null)
        {
            beamController = go.AddComponent<BeamController>();
        }

        beamController.Initialize(origin, targetInfo, lifeTime, dps, damageMask);

        return new BeamInstance
        {
            controller = beamController,
            gameObject = go
        };
    }

    /// <summary>
    /// Applies instant damage for Press/Charge modes
    /// </summary>
    private void ApplyInstantDamage(Transform origin, BeamTargetInfo targetInfo)
    {
        if (damageAmount <= 0f) return;

        Vector3 direction = (targetInfo.targetPoint - origin.position).normalized;
        
        if (Physics.Raycast(origin.position, direction, out RaycastHit hit, targetInfo.distance, damageMask))
        {
            var damageable = hit.collider.GetComponent<IDamageable>();
            damageable?.TakeDamage(damageAmount, DamageType.Bleed);
        }
    }

    /// <summary>
    /// Clean up any remaining beams when the action is disabled
    /// </summary>
    public void OnDisable()
    {
        foreach (var kvp in _activeBeams.ToList())
        {
            if (kvp.Value.controller != null)
            {
                kvp.Value.controller.ForceEnd();
            }
        }
        _activeBeams.Clear();
    }
}