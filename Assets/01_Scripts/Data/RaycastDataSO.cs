using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Projectiles/RaycastData")]
public class RaycastDataSO : ScriptableObject
{
    //public LineRenderer lineRenderer;
    public float range;
    public LayerMask hitLayers;
    public float damage;
}