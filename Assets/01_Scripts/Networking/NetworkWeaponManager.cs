using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

/// <summary>
/// Add this script to your Player prefab alongside WeaponManager
/// It syncs weapon equip/drop across network
/// </summary>
[RequireComponent(typeof(WeaponManager))]
public class NetworkWeaponManager : NetworkBehaviour
{
    private WeaponManager weaponManager;
    private WeaponContext weaponContext;
    
    private readonly SyncVar<string> currentWeaponPath = new(string.Empty);
    
    private void Awake()
    {
        weaponManager = GetComponent<WeaponManager>();
        weaponContext = GetComponent<WeaponContext>();
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        // For FishNet v4
        currentWeaponPath.OnChange += OnWeaponChanged;
        
        // If joining late, sync current weapon
        if (!string.IsNullOrEmpty(currentWeaponPath.Value))
        {
            LoadWeaponVisual(currentWeaponPath.Value);
        }
    }
    
    public override void OnStopClient()
    {
        base.OnStopClient();
        currentWeaponPath.OnChange -= OnWeaponChanged;
    }
    
    public void OnWeaponPickedUp(WeaponDataSO weaponData)
    {
        if (!IsOwner) return;
        string path = $"Weapons/{weaponData.name}";
        ServerEquipWeapon(path);
    }
    
    public void OnWeaponDropped()
    {
        if (!IsOwner) return;
        ServerDropWeapon();
    }
    
    [ServerRpc]
    private void ServerEquipWeapon(string weaponPath)
    {
        currentWeaponPath.Value = weaponPath;
        ObserversShowWeapon(weaponPath);
    }
    
    [ServerRpc]
    private void ServerDropWeapon()
    {
        currentWeaponPath.Value = string.Empty;
        
        ObserversHideWeapon();
    }
    
    [ObserversRpc]
    private void ObserversShowWeapon(string weaponPath)
    {
        if (IsOwner) return;
        LoadWeaponVisual(weaponPath);
    }
    
    [ObserversRpc]
    private void ObserversHideWeapon()
    {
        if (IsOwner) return;
        
        if (weaponContext?.WeaponController != null)
        {
            foreach (var model in weaponContext.WeaponController._models)
            {
                if (model != null) Destroy(model);
            }
            Destroy(weaponContext.WeaponController.gameObject);
            weaponContext.WeaponController = null;
        }
    }
    
    private void OnWeaponChanged(string oldValue, string newValue, bool asServer)
    {
        if (IsOwner) return;
        
        if (!string.IsNullOrEmpty(newValue))
        {
            LoadWeaponVisual(newValue);
        }
        else
        {
            ObserversHideWeapon();
        }
    }
    
    private void LoadWeaponVisual(string weaponPath)
    {
        var weaponData = Resources.Load<WeaponDataSO>(weaponPath);
        if (weaponData == null) return;
        
        var weaponObject = new GameObject($"WeaponController_{weaponData.weaponName}");
        weaponObject.transform.SetParent(weaponManager.transform, false);
        
        var controller = weaponObject.AddComponent<WeaponController>();
        controller.InitializeVisualOnly(weaponData, weaponContext);
        
        weaponContext.WeaponController = controller;
    }
}