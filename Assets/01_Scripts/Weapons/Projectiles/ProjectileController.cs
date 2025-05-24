using UnityEngine;
using System.Collections.Generic;

public class ProjectileController : MonoBehaviour
{
    [HideInInspector] public GameObject owner;
    [HideInInspector] public LayerMask hitLayer;
    public float speed     = 15f;
    public float lifetime  = 5f;
    public bool  useGravity = false;

    [SerializeReference]
    public List<ProjectileActionData> onHitActions = new();

    Rigidbody rb;

    public void Initialize(GameObject owner)
    {
        this.owner = owner;
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = useGravity;
            rb.linearVelocity   = transform.forward * speed;
        }
        
        if (lifetime > 0)
            Destroy(gameObject, lifetime);
    }

    /*
    private void OnTriggerEnter(Collider other)
    {
        // ignore self-hits
        if (other.gameObject == owner) return;

        // build CollisionInfo
        var info = new CollisionInfo(other);

        // execute all actions
        foreach (var act in onHitActions)
            act?.Execute(gameObject, info, owner);

        Destroy(gameObject);
    }
    */

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject == owner) return;
        
        // ignore non-hitlayer objects
        if ((hitLayer & (1 << col.gameObject.layer)) == 0) return;
        
        // build CollisionInfo
        var info = new CollisionInfo(col);

        // execute all actions
        foreach (var act in onHitActions)
            act?.Execute(gameObject, info, owner);
    }
}