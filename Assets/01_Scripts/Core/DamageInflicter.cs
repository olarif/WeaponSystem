using System;
using UnityEngine;

public class DamageInflicter : MonoBehaviour
{
    public float damage;
    public float impactForce;
    
    // TODO: add damage over time


    //when player collides with this collider

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player collided with damage inflicter!");
            
            other.GetComponent<PlayerController>().ResetVelocity();
            other.GetComponent<PlayerController>().ApplyForce(Vector3.back * impactForce);
            
            HealthComponent healthComponent = other.GetComponentInChildren<HealthComponent>();
            if (healthComponent != null)
            {
                //apply damage to the player
                healthComponent.TakeDamage(damage);
            }
        }
    }
}