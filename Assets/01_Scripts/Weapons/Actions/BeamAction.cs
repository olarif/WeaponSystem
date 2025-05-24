using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and manages beam behavior based on the selected mode (Press, Charge, Continuous).
/// Implements IWeaponAction to integrate with the weapon system.
/// </summary>
[Serializable]
public class BeamAction : IWeaponAction
{
    /// <summary>
    /// Determines how the beam is triggered and maintained.
    /// </summary>
    public enum Mode { Press, Charge, Continuous }

    [Header("General Settings")]
    [Tooltip("Mode of beam activation: instant press, charge then shoot, or continuous.")]
    public Mode mode = Mode.Press;

    [Tooltip("Prefab containing a BeamController component.")]
    public GameObject beamPrefab;

    [Header("Timing")]
    [Tooltip("Press/Charge: duration before beam auto‐dies.")]
    public float upTime = 0.2f;

    [Tooltip("Continuous: extra seconds after release before auto‐destroy.")]
    public float extraTime = 0.1f;

    [Header("Damage")]
    [Tooltip("Instant damage amount for Press/Charge modes.")]
    public float damageAmount = 10f;

    [Tooltip("Damage per second for Continuous mode.")]
    public float continuousDPS = 5f;

    [Tooltip("Layers that the beam can damage.")]
    public LayerMask damageMask = ~0;

    [Header("Beam Parameters")]
    [Tooltip("Length of the beam in world units.")]
    public float length = 50f;

    // Tracks active beam controllers for each action binding (continuous mode)
    private readonly Dictionary<ActionBindingData, BeamController> _activeBeams
        = new Dictionary<ActionBindingData, BeamController>();

    /// <summary>
    /// Called by the WeaponController when an action binding triggers.
    /// Spawns or ends beams based on mode and trigger phase.
    /// </summary>
    public void Execute(WeaponContext ctx, InputBindingData ib, ActionBindingData ab)
    {
        if (beamPrefab == null)
            return;

        var origin = ctx.GetFirePointsFor(ib.hand).FirstOrDefault();
        if (origin == null)
            return;

        switch (mode)
        {
            // Instant and charged beams fire on perform
            case Mode.Press:
            case Mode.Charge:
                if (ab.triggerPhase != TriggerPhase.OnPerform)
                    return;

                SpawnBeam(origin, length, upTime, 0f);
                ApplyInstantDamage(origin);
                break;

            // Continuous beams start on OnStart and end on OnCancel
            case Mode.Continuous:
                if (ab.triggerPhase == TriggerPhase.OnStart)
                {
                    ib.activeBeam = SpawnBeam(origin, length, Mathf.Infinity, continuousDPS);
                }
                else if (ab.triggerPhase == TriggerPhase.OnCancel)
                {
                    ib.activeBeam?.EndBeam(extraTime);
                    ib.activeBeam = null;
                }
                break;
        }
    }

    /// <summary>
    /// Instantiates the beam prefab and configures its controller.
    /// </summary>
    private BeamController SpawnBeam(
        Transform origin,
        float beamLength,
        float lifeTime,
        float dps)
    {
        var go = GameObject.Instantiate(beamPrefab, origin.position, origin.rotation);
        var bc = go.GetComponent<BeamController>();

        bc.origin = origin;
        bc.length = beamLength;
        bc.lifetime = lifeTime;
        bc.autoDestroy = lifeTime < Mathf.Infinity;
        bc.damagePerSecond = dps;
        bc.damageMask = damageMask;

        return bc;
    }

    /// <summary>
    /// Performs an immediate raycast damage on Press/Charge modes.
    /// </summary>
    private void ApplyInstantDamage(Transform origin)
    {
        if (damageAmount <= 0f)
            return;

        if (Physics.Raycast(
            origin.position,
            origin.forward,
            out var hit,
            length,
            damageMask))
        {
            var target = hit.collider.GetComponent<IDamageable>();
            target?.TakeDamage(damageAmount, DamageType.Bleed);
        }
    }
}
