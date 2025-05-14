using UnityEngine;

public struct CollisionInfo
{
    public Vector3 Point;       // impact position
    public Vector3 Normal;      // surface normal
    public GameObject HitObject;// the thing we collided with

    public CollisionInfo(RaycastHit hit)
        => (Point, Normal, HitObject) = (hit.point, hit.normal, hit.collider.gameObject);

    public CollisionInfo(Collision col)
        => (Point, Normal, HitObject) = (col.contacts[0].point, col.contacts[0].normal, col.collider.gameObject);
}