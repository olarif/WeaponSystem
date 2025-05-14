using UnityEngine;

[CreateAssetMenu (menuName = "Projectiles/ProjectileData")]
public class ProjectileDataSO : ScriptableObject
{
    public GameObject projectilePrefab;
    public LayerMask collisionMask;
    public float speed = 10f;
    public float damage = 10f;
    public float lifetime = 5f;

    public float turnSpeed; //for guided projectiles
    public bool enableGravity; //for gravity projectiles
    public int maxBounces; //for ricochet projectiles

}