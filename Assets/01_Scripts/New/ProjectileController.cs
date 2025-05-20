using UnityEngine;
using System.Collections.Generic;

public class ProjectileController : MonoBehaviour
{
    [HideInInspector] public GameObject owner;
    public float speed     = 15f;
    public float lifetime  = 5f;
    public bool  useGravity= false;

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
        Destroy(gameObject, lifetime);
    }

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

    // if you want to handle physics collisions instead of triggers:
    private void OnCollisionEnter(Collision col)
    {
        // ignore self-hits
        if (col.gameObject == owner) return;

        // build CollisionInfo
        var info = new CollisionInfo(col);

        // execute all actions
        foreach (var act in onHitActions)
            act?.Execute(gameObject, info, owner);

        Destroy(gameObject);
    }
}