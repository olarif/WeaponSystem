using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

public class ProjectileController : MonoBehaviour
{
    [HideInInspector] public GameObject owner;
    [HideInInspector] public LayerMask hitLayer;
    
    [Header("Base Settings")]
    public float speed = 15f;
    public float lifetime = 5f;
    public bool useGravity = false;

    [Header("Projectile Components")]
    [SerializeReference]
    public List<ProjectileComponent> components = new();

    [Header("Runtime Info")]
    [SerializeField, ReadOnly] private bool isDestroyed = false;
    [SerializeField, ReadOnly] private float timeAlive = 0f;
    [SerializeField, ReadOnly] private Vector3 lastPosition;

    // Runtime data that components can access
    public ProjectileRuntimeData RuntimeData { get; private set; }
    
    private Rigidbody rb;
    private List<ProjectileComponent> activeComponents = new();

    public void Initialize(GameObject owner, Vector3 initialVelocity)
    {
        this.owner = owner;
        lastPosition = transform.position;
        
        // Initialize runtime data
        RuntimeData = new ProjectileRuntimeData
        {
            projectile = this,
            owner = owner,
            initialVelocity = initialVelocity,
            currentVelocity = initialVelocity,
            hitTargets = new HashSet<GameObject>(),
            customData = new Dictionary<string, object>()
        };

        // Setup rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = useGravity;
            rb.linearVelocity = initialVelocity;
        }

        // Initialize all components
        InitializeComponents();
        
        // Start lifetime countdown
        if (lifetime > 0)
            Destroy(gameObject, lifetime);
    }

    private void InitializeComponents()
    {
        activeComponents.Clear();
        
        foreach (var component in components)
        {
            if (component != null)
            {
                try
                {
                    component.Initialize(RuntimeData);
                    activeComponents.Add(component);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to initialize projectile component {component.GetType().Name}: {e.Message}");
                }
            }
        }
    }

    private void Update()
    {
        if (isDestroyed) return;

        timeAlive += Time.deltaTime;
        UpdateRuntimeData();
        
        // Update all active components
        for (int i = activeComponents.Count - 1; i >= 0; i--)
        {
            var component = activeComponents[i];
            try
            {
                component.UpdateComponent(RuntimeData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error updating projectile component {component.GetType().Name}: {e.Message}");
                activeComponents.RemoveAt(i);
            }
        }

        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (isDestroyed) return;

        UpdateRuntimeData();
        
        // Fixed update for physics-based components
        foreach (var component in activeComponents)
        {
            component.FixedUpdateComponent(RuntimeData);
        }
    }

    private void UpdateRuntimeData()
    {
        RuntimeData.timeAlive = timeAlive;
        RuntimeData.currentPosition = transform.position;
        RuntimeData.lastPosition = lastPosition;
        RuntimeData.currentVelocity = rb != null ? rb.linearVelocity : Vector3.zero;
        RuntimeData.distanceTraveled += Vector3.Distance(transform.position, lastPosition);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(new CollisionInfo(collision));
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(new CollisionInfo(other));
    }

    private void HandleCollision(CollisionInfo collisionInfo)
    {
        if (isDestroyed) return;
        if (collisionInfo.HitObject == owner) return;
        
        // Check layer mask
        if ((hitLayer & (1 << collisionInfo.HitObject.layer)) == 0) return;

        // Update runtime data with collision info
        RuntimeData.lastCollision = collisionInfo;
        RuntimeData.hitTargets.Add(collisionInfo.HitObject);

        // Let components handle the collision
        bool shouldDestroy = false;
        foreach (var component in activeComponents)
        {
            var result = component.OnCollision(RuntimeData, collisionInfo);
            if (result == ComponentResult.DestroyProjectile)
                shouldDestroy = true;
        }

        if (shouldDestroy)
        {
            DestroyProjectile();
        }
    }

    public void DestroyProjectile()
    {
        if (isDestroyed) return;
        
        isDestroyed = true;
        
        foreach (var component in activeComponents)
        {
            component.OnDestroy(RuntimeData);
        }
        
        Destroy(gameObject);
    }

    // Public methods for components to use
    public void SetVelocity(Vector3 velocity)
    {
        if (rb != null)
            rb.linearVelocity = velocity;
        RuntimeData.currentVelocity = velocity;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
        RuntimeData.currentPosition = position;
    }
}