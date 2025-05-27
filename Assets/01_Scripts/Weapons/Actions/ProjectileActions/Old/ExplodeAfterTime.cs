/*using System.Collections;
using UnityEngine;

[System.Serializable]
public class ExplodeAfterTime : ProjectileActionData
{
    public float timeToExplode = 3f;
    public DamageType damageType;
    public GameObject explosionPrefab;
    public float damage = 5f;
    public float explosionRadius = 5f;
    public float explosionForce = 10f;
    public float duration = 2f;

    public override void Execute(GameObject projectile, CollisionInfo collision, GameObject owner)
    { 
        CoroutineRunner.Instance.StartRoutine(DelayedExplosion());

        IEnumerator DelayedExplosion()
        {
            yield return new WaitForSeconds(timeToExplode);
            TriggerExplosion(collision);
        }
    }

    private void TriggerExplosion(CollisionInfo collision)
    {
        
        Debug.Log("ExplodeAfterTime Projectile triggered");
        
        Collider[] colliders = Physics.OverlapSphere(collision.Point, explosionRadius);
        
        foreach (Collider hit in colliders)
        {
            if (hit.TryGetComponent<IDamageable>(out var d))
            {
                d.TakeDamage(damage, damageType);
            }
            
            //add explosion force to the object
            if (hit.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.AddExplosionForce(explosionForce, collision.Point, explosionRadius);
            }
            
            //if player
            if (hit.TryGetComponent<PlayerController>(out var player))
            {
                // Apply damage to the player
                //calculate force
                Vector3 forceDirection = (hit.transform.position - collision.Point).normalized;
                float forceMagnitude = explosionForce * (1 - (Vector3.Distance(collision.Point, hit.transform.position) / explosionRadius));
                
                player.ApplyForce( forceDirection * forceMagnitude);
            }
        }
        
        // Instantiate the explosion prefab at the collision point
        if (explosionPrefab != null)
        {
            GameObject explosion = Object.Instantiate(explosionPrefab, collision.Point, Quaternion.identity);
            Object.Destroy(explosion, duration);
        }
    }
}*/