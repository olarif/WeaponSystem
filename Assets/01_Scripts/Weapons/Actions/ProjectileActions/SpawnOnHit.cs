using System.Collections;
using UnityEngine;

[System.Serializable]
public class SpawnOnHit : ProjectileActionData
{
    [Tooltip("Prefab to spawn on hit")]
    public GameObject spawnPrefab;
    [Tooltip("How many to spawn")]
    public int count = 1;
    [Tooltip("Seconds before the spawned objects auto-destroy")]
    public float childLifeTime = 1f;
    [Tooltip("Local scale for each spawned object")]
    public Vector3 spawnSize = Vector3.one;
    
    public override void Execute(GameObject projectile, CollisionInfo collision, GameObject owner)
    {
        Vector3 origin = collision.Point;
        for (int i = 0; i < count; i++)
        {
            // spawn the instance
            var go = Object.Instantiate(spawnPrefab, origin, Quaternion.identity);
            go.transform.localScale = spawnSize;

            // if it’s a projectile, initialize it
            if (go.TryGetComponent<ProjectileController>(out var ctrl))
            {
                ctrl.owner    = owner;
                ctrl.lifetime = childLifeTime;
                ctrl.Initialize(owner);
            }
            
            Object.Destroy(go, childLifeTime);
        }
    }
}