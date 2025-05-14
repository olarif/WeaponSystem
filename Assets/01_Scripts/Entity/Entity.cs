using System;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [SerializeField] private string entityName = "Entity";
    
    protected HealthComponent HealthComponent;
}
