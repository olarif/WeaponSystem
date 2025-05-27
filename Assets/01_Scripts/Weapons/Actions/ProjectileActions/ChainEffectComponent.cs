using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ChainLightningComponent : ProjectileComponent
{
    [Header("Chain Settings")]
    [Tooltip("Which layers can be chained to")]
    public LayerMask targetLayers    = -1;
    [Tooltip("How many extra jumps after the first hit")]
    public int       maxChains       = 3;
    [Tooltip("Max distance between chain jumps")]
    public float     chainRange      = 8f;
    [Tooltip("Delay before each jump")]
    public float     chainDelay      = 0.1f;
    
    [Header("Bolt VFX")]
    [Tooltip("A simple bolt prefab (e.g. a small sprite or mesh)")]
    public GameObject boltPrefab;
    [Tooltip("Speed at which the bolt travels between targets")]
    public float      boltSpeed      = 20f;
    [Tooltip("Destroy the bolt object this many seconds after flight")]
    public float      boltLifetime   = 1f;
    
    [Header("Damage")]
    public DamageType damageType     = DamageType.Lightning;
    [Tooltip("Damage dealt to the first target on impact")]
    public float      initialDamage  = 10f;
    [Tooltip("Multiply damage by this factor on each subsequent jump")]
    [Range(0f,1f)]
    public float      damageFalloff  = 0.8f;

    public override ComponentResult OnCollision(
        ProjectileRuntimeData data,
        CollisionInfo collision)
    {
        if (!isEnabled) return ComponentResult.Continue;

        // 1) Damage the very first hit
        var first = collision.HitObject;
        if (first != null && first.TryGetComponent<IDamageable>(out var dmg))
            dmg.TakeDamage(initialDamage, damageType);

        // 2) Mark it as hit so we don’t retarget it
        if (first != null)
            data.hitTargets.Add(first);

        // 3) Kick off the chain coroutine
        CoroutineRunner.Instance.StartRoutine(
            ChainRoutine(
                data,
                fromPoint: collision.Point,
                lastTarget: first,
                damageToDeal: initialDamage * damageFalloff,
                chainCount: 0
            )
        );

        // 4) Destroy the projectile itself immediately
        return ComponentResult.DestroyProjectile;
    }

    private IEnumerator ChainRoutine(
        ProjectileRuntimeData data,
        Vector3         fromPoint,
        GameObject      lastTarget,
        float           damageToDeal,
        int             chainCount
    ) {
        // Continue jumping until we exhaust maxChains
        while (chainCount < maxChains)
        {
            yield return new WaitForSeconds(chainDelay);

            // find the next target not yet hit
            var next = Physics
                .OverlapSphere(fromPoint, chainRange, targetLayers)
                .Select(c => c.gameObject)
                .Where(g => !data.hitTargets.Contains(g))
                .Where(g => g.TryGetComponent<IDamageable>(out _))
                .OrderBy(g => Vector3.Distance(fromPoint, g.transform.position))
                .FirstOrDefault();

            if (next == null)
                yield break; // no more valid targets

            data.hitTargets.Add(next);

            // spawn & fly the bolt prefab
            if (boltPrefab != null)
            {
                var bolt = UnityEngine.Object.Instantiate(
                    boltPrefab,
                    fromPoint,
                    Quaternion.LookRotation(next.transform.position - fromPoint)
                );

                float distance = Vector3.Distance(fromPoint, next.transform.position);
                float travelTime = distance / boltSpeed;
                float elapsed = 0f;

                while (elapsed < travelTime)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / travelTime);
                    bolt.transform.position =
                        Vector3.Lerp(fromPoint, next.transform.position, t);
                    yield return null;
                }

                bolt.transform.position = next.transform.position;
                UnityEngine.Object.Destroy(bolt, boltLifetime);
            }

            // apply damage on arrival
            if (next.TryGetComponent<IDamageable>(out var dmg2))
                dmg2.TakeDamage(damageToDeal, damageType);

            // prepare for the next jump
            fromPoint     = next.transform.position;
            damageToDeal *= damageFalloff;
            chainCount++;
        }
    }
}
