using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class SpawnOnHit : ProjectileActionData
{
    public GameObject spawnPrefab;
    public int count = 1;
    public float spreadAngle = 30f;
    public float childLifeTime = 3f;
    
    public override void Execute(GameObject projectile, CollisionInfo collision, GameObject owner)
    {
        Vector3 origin = collision.Point;
        for (int i = 0; i < count; i++)
        {
            float half = spreadAngle * 0.5f;
            Quaternion rot = Quaternion.Euler(
                Random.Range(-half, half),
                Random.Range(-half, half),
                Random.Range(-half, half)
            ) * projectile.transform.rotation;

            var go = Object.Instantiate(spawnPrefab, origin, rot);
            if (go.TryGetComponent<ProjectileController>(out var ctrl))
            {
                ctrl.owner    = owner;
                ctrl.lifetime = childLifeTime;
                ctrl.Initialize(owner);
            }
        }
    }
}