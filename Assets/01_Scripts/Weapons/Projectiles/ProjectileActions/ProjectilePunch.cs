using UnityEngine;

[System.Serializable]
public class ProjectilePunch : ProjectileActionData
{
    
    
    public override void Execute(GameObject projectile, CollisionInfo collision, GameObject owner)
    {
        Debug.Log("ProjectilePunch Projectile triggered");
        
        //give the projectile some sway with iTween.Punch
        iTween.PunchPosition(projectile, new Vector3 (
            Random.Range(-1, 
            1), 
            Random.Range(-1, 
                1), 0), 
            3);
    }
}