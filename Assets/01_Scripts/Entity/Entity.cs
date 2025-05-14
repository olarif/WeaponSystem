using System;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [SerializeField] private string entityName = "Entity";

    //protected HealthComponent healthComponent;
    //protected EntityStats stats;

    protected virtual void Awake()
    {
        //healthComponent = GetComponent<HealthComponent>() ?? gameObject.AddComponent<HealthComponent>();
        //stats = GetComponent<EntityStats>() ?? gameObject.AddComponent<EntityStats>();

        if (string.IsNullOrEmpty(entityName))
        {
            entityName = gameObject.name;
        }
    }
}
