/*using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class ProjectilePunch : ProjectileActionData
{
    
   
    public override void Execute(GameObject projectile, CollisionInfo collision, GameObject owner)
    {
        Debug.Log("ProjectilePunch Projectile triggered");
        
        //give the projectile some sway with dotween
        
        if (projectile != null)
        {
            // Apply a punch effect to the projectile
            projectile.transform.DOPunchPosition(new Vector3(0.1f, 0, 0), 0.5f, 10, 1f)
                .OnComplete(() => Debug.Log("Punch effect completed on projectile"));
        }
        else
        {
            Debug.LogWarning("Projectile is null, cannot apply punch effect.");
        }
        
    }
}*/