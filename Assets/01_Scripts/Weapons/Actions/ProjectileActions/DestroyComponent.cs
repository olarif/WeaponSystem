using System.Collections;
using UnityEngine;

[System.Serializable]
public class DestroyComponent : ProjectileComponent
{
    [Tooltip("Delay before destroying the projectile")]
    public float delay = 2f;
    [Tooltip("Destroy on collision event")]
    public bool destroyOnCollision = true;
    
    public override ComponentResult OnCollision(ProjectileRuntimeData data, CollisionInfo ci)
    {
        if (destroyOnCollision)
            CoroutineRunner.Instance.StartRoutine(DelayedDestroy(data.projectile));

        return ComponentResult.Continue;
    }

    // Waits for delay then destroys the projectile
    private IEnumerator DelayedDestroy(ProjectileController controller)
    {
        yield return new WaitForSeconds(delay);
        controller.DestroyProjectile();
    }
}