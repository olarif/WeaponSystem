using UnityEngine;

public struct CollisionInfo
{
    public Vector3 Point;       // impact position
    public GameObject HitObject;// the thing we collided with
    
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
}