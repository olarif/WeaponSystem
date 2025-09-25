using UnityEngine;
using FishNet.Object;
using System.Collections.Generic;

/// <summary>
/// Add this script to your Player prefab
/// Makes projectiles visible to all players
/// </summary>
public class SimpleNetworkProjectileSpawner : NetworkBehaviour
{
    [Header("IMPORTANT: Add ALL projectile prefabs here!")]
    [SerializeField] private List<GameObject> projectilePrefabs = new List<GameObject>();
    
    private Dictionary<string, int> prefabNameToIndex = new Dictionary<string, int>();
    
    private void Awake()
    {
        // Build lookup dictionary
        for (int i = 0; i < projectilePrefabs.Count; i++)
        {
            if (projectilePrefabs[i] != null)
            {
                prefabNameToIndex[projectilePrefabs[i].name] = i;
            }
        }
    }
    
    public void SpawnNetworkedProjectile(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        if (!IsOwner) return;
        
        if (!prefabNameToIndex.TryGetValue(prefab.name, out int index))
        {
            Debug.LogError($"Projectile {prefab.name} not registered! Add it to the projectilePrefabs list.");
            return;
        }
        
        ServerSpawnProjectile(index, position, rotation, velocity);
    }
    
    [ServerRpc]
    private void ServerSpawnProjectile(int prefabIndex, Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        if (prefabIndex < 0 || prefabIndex >= projectilePrefabs.Count) return;
        
        GameObject prefab = projectilePrefabs[prefabIndex];
        if (prefab == null) return;
        
        ObserversSpawnProjectile(prefabIndex, position, rotation, velocity);
    }
    
    [ObserversRpc]
    private void ObserversSpawnProjectile(int prefabIndex, Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        if (prefabIndex < 0 || prefabIndex >= projectilePrefabs.Count) return;
        
        GameObject prefab = projectilePrefabs[prefabIndex];
        if (prefab == null) return;
        
        var projectile = Instantiate(prefab, position, rotation);
        
        var controller = projectile.GetComponent<ProjectileController>();
        if (controller != null)
        {
            controller.Initialize(gameObject, velocity);
        }
    }
}