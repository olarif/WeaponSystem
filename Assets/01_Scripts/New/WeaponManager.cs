using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    private PlayerInput _actions;
    
    [SerializeField] Transform weaponHolder;
    public GameObject worldPickupPrefab;
    WeaponController equipped;
    WeaponDataSO equippedData;

    public bool CanEquipWeapon => equipped == null;
    private void Awake()
    {
        _actions = new PlayerInput();
    }
    private void OnEnable() => _actions.Enable();
    private void OnDisable() => _actions.Disable();
    
    public void EquipFromPickup(WeaponPickup pickup)
    {
        if (!CanEquipWeapon) return;

        var go = new GameObject("WeaponController_" + pickup.Data.weaponName);
        go.transform.SetParent(weaponHolder, false);

        var wc  = go.AddComponent<WeaponController>();
        var ctx = GetComponent<WeaponContext>();
        wc.Initialize(pickup.Data, ctx);

        equipped = wc;
        equippedData = pickup.Data;
    }
    
    public void DropWeapon()
    {
        if (equipped == null) return;
        
        var worldPickup = Instantiate(worldPickupPrefab, equipped.transform.position, equipped.transform.rotation);
        
        var weaponPickup = worldPickup.GetComponent<WeaponPickup>();
        if (weaponPickup != null)
        {
            weaponPickup.Initialize(equippedData);
            weaponPickup.transform.SetParent(null, true);
        }
        
    }
}