using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class SpawnAfterTime : ProjectileActionData
{
    public float timeToSpawn = 0.5f;
    public GameObject spawnPrefab;
    public int count = 1;
    public float lifetime = 2f;
    public Vector3 spawnSize = Vector3.one;
    
    
    public override void Execute(GameObject projectile, CollisionInfo collision, GameObject owner)
    {
        CoroutineRunner.Instance.StartRoutine(DelayedSpawn());

        IEnumerator DelayedSpawn()
        {
            yield return new WaitForSeconds(timeToSpawn);
            SpawnVFX(projectile);
        }
    }

    private void SpawnVFX(GameObject projectile)
    {
        if (spawnPrefab == null) return;

        Vector3 origin   = projectile.transform.position;
        Quaternion rotation = projectile.transform.rotation;

        for (int i = 0; i < count; i++)
        {
            var go = Object.Instantiate(spawnPrefab, origin, rotation);
            go.transform.localScale = spawnSize;
            
            Object.Destroy(go, lifetime);
                
        }
    }
}