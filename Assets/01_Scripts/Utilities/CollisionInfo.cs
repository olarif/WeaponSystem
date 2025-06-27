using UnityEngine;

/// <summary>
/// Holds collision details: impact point and GameObject hit.
/// </summary>
public struct CollisionInfo
{
    public Vector3 Point;       // impact position
    public GameObject HitObject;// the object we collided with
    
    public CollisionInfo(RaycastHit hit)
    {
        Point = hit.point;
        HitObject = hit.collider.gameObject;
    }
    
    public CollisionInfo(Collision col)
    {
        Point = col.contacts[0].point;
        HitObject = col.gameObject;
    }
    
    public CollisionInfo(Collider col)
    {
        Point = col.bounds.center;
        HitObject = col.gameObject;
    }
}