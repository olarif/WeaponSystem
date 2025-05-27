using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnPrefabComponent : ProjectileComponent
{
    [Tooltip("Which event(s) to spawn on")]
    public List<ProjectileEvent> triggerEvents = new List<ProjectileEvent>
    {
        ProjectileEvent.OnInitialize,
        ProjectileEvent.OnUpdate,
        ProjectileEvent.OnFixedUpdate,
        ProjectileEvent.OnCollision,
        ProjectileEvent.OnDestroy
    };

    [Tooltip("Prefab to spawn")]
    public GameObject prefab;

    [Tooltip("How many")]
    public int count = 1;

    [Tooltip("Cone spread in degrees, if >0")]
    public float spreadAngle = 0f;

    [Tooltip("Delay before spawn (seconds)")]
    public float delay = 0f;

    [Tooltip("Lifetime of the spawned object")]
    public float lifetime = 2f;

    [Tooltip("Scale of each spawn")]
    public Vector3 scale = Vector3.one;

    public override void Initialize(ProjectileRuntimeData data)
    {
        if (triggerEvents.Contains(ProjectileEvent.OnInitialize))
            TrySpawn(data);
    }

    public override void UpdateComponent(ProjectileRuntimeData data)
    {
        if (triggerEvents.Contains(ProjectileEvent.OnUpdate))
            TrySpawn(data);
    }

    public override void FixedUpdateComponent(ProjectileRuntimeData data)
    {
        if (triggerEvents.Contains(ProjectileEvent.OnFixedUpdate))
            TrySpawn(data);
    }

    public override ComponentResult OnCollision(ProjectileRuntimeData data, CollisionInfo ci)
    {
        if (triggerEvents.Contains(ProjectileEvent.OnCollision))
            TrySpawn(data, ci.Point);
        return ComponentResult.Continue;
    }

    public override void OnDestroy(ProjectileRuntimeData data)
    {
        if (triggerEvents.Contains(ProjectileEvent.OnDestroy))
            TrySpawn(data);
    }

    private void TrySpawn(ProjectileRuntimeData data, Vector3? point = null, Vector3? normal = null)
    {
        data.projectile.StartCoroutine( DoSpawn(data, point, normal) );
    }

    private IEnumerator DoSpawn(
        ProjectileRuntimeData data,
        Vector3? point, Vector3? normal
    ) {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        Vector3 origin    = point ?? data.currentPosition;
        Quaternion basis  = Quaternion.identity;
        if (normal.HasValue)
            basis = Quaternion.LookRotation(normal.Value);
        else
            basis = data.projectile.transform.rotation;

        for (int i = 0; i < count; i++)
        {
            Quaternion rot = basis;
            if (spreadAngle > 0f)
            {
                float h = spreadAngle * .5f;
                rot = Quaternion.Euler(
                    UnityEngine.Random.Range(-h,h),
                    UnityEngine.Random.Range(-h,h),
                    UnityEngine.Random.Range(-h,h)
                ) * basis;
            }

            var go = UnityEngine.Object.Instantiate(prefab, origin, rot);
            go.transform.localScale = scale;
            if (lifetime > 0f)
                UnityEngine.Object.Destroy(go, lifetime);
        }
    }
}