using UnityEngine;

[CreateAssetMenu(menuName = "Projectiles/AoEData")]
public class AoEDataSO : ScriptableObject
{
    public float radius;
    public float damage;
    public float duration;
    
    public LayerMask targetLayer;
    
    public GameObject explosionPrefab;
}