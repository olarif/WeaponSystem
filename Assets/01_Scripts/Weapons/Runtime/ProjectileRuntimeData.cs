using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProjectileRuntimeData
{
    public ProjectileController projectile;
    public GameObject owner;
    
    // Motion data
    public Vector3 initialVelocity;
    public Vector3 currentVelocity;
    public Vector3 currentPosition;
    public Vector3 lastPosition;
    
    // State data
    public float timeAlive;
    public float distanceTraveled;
    public HashSet<GameObject> hitTargets;
    public CollisionInfo lastCollision;
    
    public Dictionary<string, object> customData;
    
    // Helper methods
    public T GetCustomData<T>(string key, T defaultValue = default)
    {
        if (customData.TryGetValue(key, out var value) && value is T)
            return (T)value;
        return defaultValue;
    }
    
    public void SetCustomData<T>(string key, T value)
    {
        customData[key] = value;
    }
}