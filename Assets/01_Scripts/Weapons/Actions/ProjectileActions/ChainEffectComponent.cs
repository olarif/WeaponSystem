using System.Linq;
using UnityEngine;

[System.Serializable]
public class ChainLightningComponent : ProjectileComponent
{
    [Header("Chain Settings")]
    public LayerMask targetLayers = -1;
    public int maxChains = 3;
    public float chainRange = 8f;
    public float chainDelay = 0.1f;
    public GameObject chainEffectPrefab;
    public DamageType damageType = DamageType.Lightning;
    public float damage = 10f;
    [Tooltip("Damage multiplier for each subsequent chain (0.8 = 80% damage)")]
    public float damageReduction = 0.8f;

    public override ComponentResult OnCollision(ProjectileRuntimeData data, CollisionInfo collision)
    {
        if (!isEnabled) return ComponentResult.Continue;

        // Deal damage to the first target
        GameObject firstTarget = collision.HitObject;
        if (firstTarget != null && firstTarget.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage, damageType);
        }

        // Add first target to hit list
        if (firstTarget != null && !data.hitTargets.Contains(firstTarget))
        {
            data.hitTargets.Add(firstTarget);
        }

        // Start chaining from the first target
        CoroutineRunner.Instance.StartRoutine(ProcessChainSequence(data, firstTarget, damage * damageReduction));

        return ComponentResult.DestroyProjectile;
    }

    private System.Collections.IEnumerator ProcessChainSequence(ProjectileRuntimeData data, GameObject currentTarget, float currentDamage)
    {
        int chainsPerformed = 0;
        
        while (chainsPerformed < maxChains && currentTarget != null)
        {
            // Find next target from current target's position
            var nextTarget = FindNextChainTarget(data, currentTarget.transform.position);
            
            if (nextTarget == null)
            {
                // No more targets found, end the chain
                break;
            }

            // Add the next target to hit list immediately to prevent retargeting
            if (!data.hitTargets.Contains(nextTarget))
            {
                data.hitTargets.Add(nextTarget);
            }

            // Wait for chain delay
            yield return new WaitForSeconds(chainDelay);

            // Create and launch chain effect from current to next target
            CreateTravelingChainEffect(currentTarget.transform.position, nextTarget.transform.position, nextTarget, currentDamage);
            
            // Wait for the effect to travel (based on distance and speed)
            float travelDistance = Vector3.Distance(currentTarget.transform.position, nextTarget.transform.position);
            float travelTime = travelDistance / 20f; // Assuming default travel speed of 20
            yield return new WaitForSeconds(travelTime + 0.1f); // Small buffer
            
            // Update for next iteration - IMPORTANT: use nextTarget as the new current
            currentTarget = nextTarget;
            currentDamage *= damageReduction;
            chainsPerformed++;
        }
    }

    private GameObject FindNextChainTarget(ProjectileRuntimeData data, Vector3 fromPosition)
    {
        // Find all potential targets in range
        var allColliders = Physics.OverlapSphere(fromPosition, chainRange, targetLayers);
        
        var potentialTargets = allColliders
            .Select(c => c.gameObject)
            .Where(t => !data.hitTargets.Contains(t))
            .Where(t => t.GetComponent<IDamageable>() != null) // Only target damageable objects
            .OrderBy(t => Vector3.Distance(fromPosition, t.transform.position))
            .ToList();

        return potentialTargets.Count > 0 ? potentialTargets[0] : null;
    }

    private void CreateTravelingChainEffect(Vector3 from, Vector3 to, GameObject target, float damage)
    {
        if (chainEffectPrefab != null)
        {
            var effect = Object.Instantiate(chainEffectPrefab, from, Quaternion.LookRotation(to - from));
            
            // Start the traveling effect
            var travelComponent = effect.GetComponent<ChainLightningEffectController>();
            if (travelComponent == null)
            {
                travelComponent = effect.AddComponent<ChainLightningEffectController>();
            }
            
            travelComponent.StartTravel(from, to, target, damage, damageType);
            
            // Clean up after travel is complete
            Object.Destroy(effect, 3f);
        }
    }
}