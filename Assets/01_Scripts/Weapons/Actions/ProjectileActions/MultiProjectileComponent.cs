using UnityEngine;

[System.Serializable]
public class MultiProjectileComponent : ProjectileComponent
{
    [Header("Multi-Projectile Settings")]
    public int projectileCount = 3;
    public float spreadAngle = 15f;
    public float delayBetweenShots = 0.1f;
    public GameObject projectilePrefab;
    
    private bool hasTriggered = false;

    public override void Initialize(ProjectileRuntimeData data)
    {
        if (!hasTriggered && projectilePrefab != null)
        {
            CoroutineRunner.Instance.StartRoutine(SpawnAdditionalProjectiles(data));
            hasTriggered = true;
        }
    }

    private System.Collections.IEnumerator SpawnAdditionalProjectiles(ProjectileRuntimeData data)
    {
        for (int i = 1; i < projectileCount; i++)
        {
            yield return new WaitForSeconds(delayBetweenShots);

            if (data.projectile == null) break;

            // Calculate spread direction
            float angleOffset = (i - (projectileCount - 1) * 0.5f) * spreadAngle;
            Vector3 spreadDirection = Quaternion.AngleAxis(angleOffset, Vector3.up) * data.initialVelocity.normalized;
            Vector3 spreadVelocity = spreadDirection * data.initialVelocity.magnitude;

            // Spawn new projectile
            var newProjectile = Object.Instantiate(projectilePrefab, data.currentPosition, Quaternion.LookRotation(spreadDirection));
            if (newProjectile.TryGetComponent<ProjectileController>(out var controller))
            {
                controller.Initialize(data.owner, spreadVelocity);
            }
        }
    }
}