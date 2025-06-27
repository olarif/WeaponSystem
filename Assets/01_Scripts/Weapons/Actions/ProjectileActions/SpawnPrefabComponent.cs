using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns configured prefab instances on specified projectile events.
/// </summary>
[System.Serializable]
public class SpawnPrefabComponent : ProjectileComponent
{
    [Tooltip("Events that trigger spawning (e.g., OnCollision, OnDestroy)")]
    public List<ProjectileEvent> triggerEvents = new List<ProjectileEvent>
    {
        ProjectileEvent.OnCollision
    };

    [Tooltip("Prefab to instantiate")]
    public GameObject prefab;

    [Tooltip("Number of copies to spawn")]
    public int count = 1;

    [Tooltip("Cone spread angle in degrees")]
    public float spreadAngle = 0f;

    [Tooltip("Seconds to wait before spawning")]
    public float delay = 0f;

    [Tooltip("Lifetime of spawned instances in seconds")]
    public float lifetime = 2f;

    [Tooltip("Local scale applied to each instance")]
    public Vector3 scale = Vector3.one;

    public override void Initialize(ProjectileRuntimeData data)
    {
        if (triggerEvents.Contains(ProjectileEvent.OnInitialize))
            data.projectile.StartCoroutine(SpawnRoutine(data));
    }

    public override void UpdateComponent(ProjectileRuntimeData data)
    {
        if (triggerEvents.Contains(ProjectileEvent.OnUpdate))
            data.projectile.StartCoroutine(SpawnRoutine(data));
    }

    public override void FixedUpdateComponent(ProjectileRuntimeData data)
    {
        if (triggerEvents.Contains(ProjectileEvent.OnFixedUpdate))
            data.projectile.StartCoroutine(SpawnRoutine(data));
    }

    public override ComponentResult OnCollision(ProjectileRuntimeData data, CollisionInfo ci)
    {
        if (triggerEvents.Contains(ProjectileEvent.OnCollision))
            data.projectile.StartCoroutine(SpawnRoutine(data, ci.Point, ci.HitObject.transform.rotation));
        return ComponentResult.Continue;
    }

    public override void OnDestroy(ProjectileRuntimeData data)
    {
        if (triggerEvents.Contains(ProjectileEvent.OnDestroy))
            data.projectile.StartCoroutine(SpawnRoutine(data));
    }
    
    /// Coroutine that waits, spawns prefabs, applies spread, scale, and destroys after lifetime.
    private IEnumerator SpawnRoutine(
        ProjectileRuntimeData data,
        Vector3? position = null,
        Quaternion? rotation = null)
    {
        // Wait delay if specified
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        Vector3 origin = position ?? data.currentPosition;
        Quaternion basis = rotation ?? data.projectile.transform.rotation;

        for (int i = 0; i < count; i++)
        {
            // Apply random spread
            Quaternion spawnRot = basis;
            if (spreadAngle > 0f)
            {
                float half = spreadAngle * 0.5f;
                spawnRot *= Quaternion.Euler(
                    Random.Range(-half, half),
                    Random.Range(-half, half),
                    Random.Range(-half, half)
                );
            }

            GameObject instance = Object.Instantiate(prefab, origin, spawnRot);
            instance.transform.localScale = scale;
            if (lifetime > 0f)
                Object.Destroy(instance, lifetime);
        }
    }
}
