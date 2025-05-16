using System;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [SerializeField] private string entityName = "Entity";

    public virtual void Heal(float amount) { }
    public virtual void Damage(float amount){ }
    public virtual bool IsAlive(){ return false; }
    public virtual void RestoreStamina(float amount){ }
    public virtual void ConsumeStamina(float amount){ }
}
