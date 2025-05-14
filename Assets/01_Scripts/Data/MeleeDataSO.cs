using UnityEngine;

[CreateAssetMenu(menuName = "Projectiles/MeleeData")]
public class MeleeDataSO : ScriptableObject
{
    public LayerMask targetLayer;
    public float damage;
    public float range;
    public float cooldown;
    public float attackSpeed;
    public float knockbackForce;
    public float knockbackDuration;
}