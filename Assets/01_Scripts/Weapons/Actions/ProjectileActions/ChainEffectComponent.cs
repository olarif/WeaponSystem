using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[Serializable]
public class ChainLightningComponent : ProjectileComponent
{
    [Header("Chain Settings")]
    public LayerMask targetLayers = -1;
    public int maxChains = 3;
    public float chainRange = 8f;
    public float chainDelay = 0.1f;

    [Header("Bolt VFX")]
    public GameObject boltPrefab;
    public float boltSpeed = 20f;
    public float boltLifetime = 1f;

    [Header("Damage")]
    public DamageType damageType = DamageType.Lightning;
    public float initialDamage = 10f;
    [Range(0f, 1f)] public float damageFalloff = 0.8f;

    // handle initial hit and start chaining
    public override ComponentResult OnCollision(ProjectileRuntimeData data, CollisionInfo collision)
    {
        if (!isEnabled) return ComponentResult.Continue;

        ApplyDamage(collision.HitObject, initialDamage);
        data.hitTargets.Add(collision.HitObject);

        CoroutineRunner.Instance.StartRoutine(
            ChainRoutine(data, collision.Point, initialDamage * damageFalloff)
        );

        return ComponentResult.DestroyProjectile;
    }

    // jump and deal damage between targets
    private IEnumerator ChainRoutine(ProjectileRuntimeData data, Vector3 origin, float damage)
    {
        for (int i = 0; i < maxChains; i++)
        {
            yield return new WaitForSeconds(chainDelay);

            var next = Physics.OverlapSphere(origin, chainRange, targetLayers)
                .Select(c => c.gameObject)
                .FirstOrDefault(g => !data.hitTargets.Contains(g) && g.TryGetComponent<IDamageable>(out _));

            if (next == null) break;

            data.hitTargets.Add(next);
            yield return BoltFly(origin, next.transform.position);
            ApplyDamage(next, damage);

            origin = next.transform.position;
            damage *= damageFalloff;
        }
    }

    // animate bolt travelling between targets
    private IEnumerator BoltFly(Vector3 start, Vector3 end)
    {
        if (boltPrefab == null) yield break;

        var bolt = GameObject.Instantiate(boltPrefab, start, Quaternion.LookRotation(end - start));
        float distance = Vector3.Distance(start, end);
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * boltSpeed / distance;
            bolt.transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(progress));
            yield return null;
        }

        bolt.transform.position = end;
        GameObject.Destroy(bolt, boltLifetime);
    }
    
    private void ApplyDamage(GameObject target, float amount)
    {
        if (target.TryGetComponent<IDamageable>(out var dmg))
            dmg.TakeDamage(amount, damageType);
    }
}
