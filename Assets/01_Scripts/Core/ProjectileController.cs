using System;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    ProjectileDataSO _data; WeaponContext _context;
    
    private Rigidbody _rigidbody;
    private float _lifeTimer;
    private int _bouncesLeft;

    public void Initialize(ProjectileDataSO data, WeaponContext context, Vector3 direction)
    {
        _rigidbody = GetComponent<Rigidbody>();
        _data = data;
        _context = context;

        _bouncesLeft = _data.maxBounces;
        _rigidbody.useGravity = _data.enableGravity;
        
        if (_rigidbody == null)
        {
            Debug.LogError("Projectile requires a Rigidbody component!");
            return;
        }
        
        // set initial velocity
        _rigidbody.linearVelocity = direction.normalized * _data.speed;
    }
    
    private void Start()
    {
        // set initial velocity
        //_rigidbody.linearVelocity = _context.FirePoint.forward * _data.speed;
        
        // set lifetime
        //Destroy(gameObject, _data.lifetime);
    }

    private void Update()
    {
        // Handle lifetime
        _lifeTimer += Time.deltaTime;
        if (_lifeTimer >= _data.lifetime)
        {
            //OnExpire();
        }
        
        //handle guided projectiles
    }

    private void OnCollisionEnter(Collision col)
    {
        // Notify any IOnHitComponents
        foreach (var hitComp in GetComponents<IOnHitComponent>())
            hitComp.OnHit(new CollisionInfo(col));

        // Check if this layer is *in* our mask and we still have bounces left
        bool shouldBounce = ((_data.collisionMask & (1 << col.gameObject.layer)) != 0)
                            && (_bouncesLeft-- > 0);

        if (shouldBounce)
        {
            Vector3 reflectDir = Vector3.Reflect(_rigidbody.linearVelocity.normalized, col.contacts[0].normal);
            _rigidbody.linearVelocity = reflectDir * _data.speed;
        }
        else
        {
            OnExpire();
        }
    }

    private void OnExpire()
    {
        Destroy(gameObject);
    }
}