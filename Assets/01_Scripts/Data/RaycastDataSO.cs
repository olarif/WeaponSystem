using UnityEngine;

[CreateAssetMenu(menuName = "Projectiles/RaycastData")]
public class RaycastDataSO : ScriptableObject
{
    public float range;
    public LayerMask collisionMask;
    public float damage;
    //public GameObject HitEffectPrefab;
    //audio
}