using System.Collections;
using UnityEngine;

[System.Serializable]
public class DestroyComponent : ProjectileComponent
{
    [Tooltip("Delay before calling DestroyProjectile()")]
    public float delay = 2f;

    [Tooltip("Should we trigger this on collision?")]
    public bool destroyOnCollision = true;

    public override ComponentResult OnCollision(
        ProjectileRuntimeData data,
        CollisionInfo ci
    ) {
        if (!destroyOnCollision) 
            return ComponentResult.Continue;

        // schedule the controller's DestroyProjectile AFTER `delay`
        CoroutineRunner.Instance.StartRoutine(DelayedDestroy(data.projectile));
        return ComponentResult.Continue;
    }

    private IEnumerator DelayedDestroy(ProjectileController ctrl)
    {
        yield return new WaitForSeconds(delay);
        // This calls each component's OnDestroy() in order, then
        // destroys the GameObject.
        ctrl.DestroyProjectile();
    }
}