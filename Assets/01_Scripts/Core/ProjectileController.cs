using System;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    ProjectileDataSO _data; WeaponContext _context;
    
    private Rigidbody _rigidbody;
    private int _bouncesLeft;

    public void Initialize(ProjectileDataSO data, WeaponContext context, Vector3 direction)
    {
        _rigidbody = GetComponent<Rigidbody>();
        _data = data;
        _context = context;

        _bouncesLeft = _data.maxBounces;
        _rigidbody.useGravity = _data.enableGravity;
        
        // set initial velocity
        _rigidbody.linearVelocity = direction.normalized * _data.speed;
        
        // set lifetime
        Destroy(gameObject, data.lifetime);
    }

    private void OnCollisionEnter(Collision col)
    {
        foreach (var hitComp in _context.OnHitComponents)
        {
            Debug.Log( "Hit component: " + hitComp.GetType().Name + " on " + gameObject.name);
            hitComp.OnHit(new CollisionInfo(col));
        }

        // Check if this layer is *in* our mask and we still have bounces left
        bool shouldBounce = ((_data.collisionMask & (1 << col.gameObject.layer)) != 0)
                            && (_bouncesLeft-- > 0);
        
        IDamageable damageable = col.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // Apply damage to the hit object
            damageable.ApplyDamage(_data.damage);
        }

        if (shouldBounce)
        {
            Vector3 reflectDir = Vector3.Reflect(_rigidbody.linearVelocity.normalized, col.contacts[0].normal);
            _rigidbody.linearVelocity = reflectDir * _data.speed;
        }
        else
        {
            // Destroy the projectile if it doesn't bounce
            OnExpire();
        }
    }

    private void OnExpire()
    {
        Destroy(gameObject);
    }
}