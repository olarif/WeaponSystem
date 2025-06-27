using System;
using UnityEngine;

[Serializable]
public class HomingMissileComponent : ProjectileComponent
{
    [Header("Targeting Settings")]
    [Tooltip("Layer mask for finding targets")]
    public LayerMask targetLayers = -1;
    [Tooltip("Range to search for targets")]
    public float targetRange = 20f;
    [Tooltip("How often to re-acquire target (sec)")]
    public float retargetInterval = 0.3f;
    [Tooltip("Max angle to consider targets (degrees from forward direction)")]
    public float maxTargetingAngle = 60f;
    [Tooltip("How much closer a new target must be to switch (0-1, where 0.8 = 20% closer)")]
    public float targetSwitchThreshold = 0.7f;

    [Header("Homing Settings")]
    [Tooltip("Speed at which to rotate toward the target (degrees/sec)")]
    public float turnSpeed = 90f;
    [Tooltip("Delay before homing kicks in (sec)")]
    public float homingDelay = 0.1f;

    // runtime state
    private GameObject currentTarget;
    private float lastRetargetTime;
    private bool homingActive;
    private float currentTargetDistance;

    public override void Initialize(ProjectileRuntimeData data)
    {
        lastRetargetTime = Time.time;
        homingActive = false;
        currentTarget = null;
        currentTargetDistance = float.MaxValue;
    }

    public override void UpdateComponent(ProjectileRuntimeData data)
    {
        // wait until the missile has flown forward for homingDelay
        if (!homingActive && data.timeAlive >= homingDelay)
        {
            homingActive = true;
            FindBestTarget(data);
        }
    }

    public override void FixedUpdateComponent(ProjectileRuntimeData data)
    {
        if (!isEnabled || !homingActive)
            return;

        // Always check for better targets more frequently
        if (Time.time - lastRetargetTime > retargetInterval)
        {
            FindBestTarget(data);
            lastRetargetTime = Time.time;
        }

        // Validate current target is still good
        if (currentTarget == null || !currentTarget.activeInHierarchy || 
            data.hitTargets.Contains(currentTarget))
        {
            FindBestTarget(data);
        }

        if (currentTarget == null)
            return;

        // Get target center point
        Vector3 targetPoint = GetTargetCenter(currentTarget);
        
        // Home toward target
        Vector3 toTarget = (targetPoint - data.currentPosition).normalized;
        Vector3 currDir = data.currentVelocity.normalized;
        float speed = data.currentVelocity.magnitude;

        float maxRad = turnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime;
        Vector3 newDir = Vector3.RotateTowards(currDir, toTarget, maxRad, 0f);

        data.projectile.SetVelocity(newDir * speed);
        data.projectile.transform.rotation = Quaternion.LookRotation(newDir);
    }

    private void FindBestTarget(ProjectileRuntimeData data)
    {
        var colls = Physics.OverlapSphere(data.currentPosition, targetRange, targetLayers);
        GameObject bestTarget = null;
        float bestScore = float.MinValue;

        Vector3 missileForward = data.currentVelocity.normalized;
        if (missileForward == Vector3.zero)
            missileForward = data.projectile.transform.forward;

        foreach (var c in colls)
        {
            var go = c.gameObject;
            
            // Skip already hit targets
            if (data.hitTargets.Contains(go)) 
                continue;
                
            // Must be damageable
            if (!go.TryGetComponent<IDamageable>(out _))
                continue;

            Vector3 targetPos = GetTargetCenter(go);
            Vector3 toTarget = (targetPos - data.currentPosition);
            float distance = toTarget.magnitude;
            
            // Skip if too far
            if (distance > targetRange)
                continue;

            Vector3 dirToTarget = toTarget.normalized;
            float angle = Vector3.Angle(missileForward, dirToTarget);
            
            // Skip targets outside our cone of vision
            if (angle > maxTargetingAngle)
                continue;

            // Calculate targeting score (higher is better)
            // Factors: distance (closer = better), angle (straighter = better)
            float distanceScore = (targetRange - distance) / targetRange;
            float angleScore = (maxTargetingAngle - angle) / maxTargetingAngle;
            
            // Weight angle more heavily for initial targeting
            float totalScore = (distanceScore * 0.4f) + (angleScore * 0.6f);
            
            // If we already have a target, new target must be significantly better
            if (currentTarget != null && go != currentTarget)
            {
                // Require new target to be meaningfully better (not just marginally)
                float currentDistance = Vector3.Distance(data.currentPosition, GetTargetCenter(currentTarget));
                if (distance > currentDistance * targetSwitchThreshold)
                    continue;
            }

            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                bestTarget = go;
            }
        }

        // Only switch if we found a meaningfully better target
        if (bestTarget != null && bestTarget != currentTarget)
        {
            currentTarget = bestTarget;
            currentTargetDistance = Vector3.Distance(data.currentPosition, GetTargetCenter(currentTarget));
        }
    }

    private Vector3 GetTargetCenter(GameObject target)
    {
        if (target.TryGetComponent<Collider>(out var col))
            return col.bounds.center;
        return target.transform.position;
    }
}
