using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    private PlayerInput _actions;
    private InputAction _dropAction;
    
    [SerializeField] Transform weaponHolder;
    private GameObject worldPickupPrefab;
    WeaponController equipped;
    WeaponDataSO equippedData;
    WeaponContext ctx;

    public bool CanEquipWeapon => equipped == null;
    private void Awake()
    {
        _actions = new PlayerInput();
        _dropAction = _actions.Player.DropWeapon;
        _dropAction.Enable();
        _dropAction.performed += OnDropPerformed;
    }
    private void OnEnable() => _actions.Enable();
    private void OnDisable() => _actions.Disable();
    
    private void OnDropPerformed(InputAction.CallbackContext context) { DropWeapon(); }
    
    public void EquipFromPickup(WeaponPickup pickup)
    {
        if (!CanEquipWeapon) return;

        var go = new GameObject("WeaponController_" + pickup.Data.weaponName);
        go.transform.SetParent(weaponHolder, false);

        var wc  = go.AddComponent<WeaponController>();
        ctx = GetComponent<WeaponContext>();
        
        ctx.WeaponManager    = this;
        wc.Initialize(pickup.Data, ctx);

        equipped = wc;
        equippedData = pickup.Data;
        worldPickupPrefab = pickup.Data.pickupPrefab;
    }
    
    public void DropWeapon()
    {
        if (equipped == null) return;
        
        Instantiate(worldPickupPrefab, transform.position, Quaternion.identity);

        Destroy(ctx.WeaponController._model);
        Destroy(ctx.WeaponController.gameObject);
        
        worldPickupPrefab = null;
        equippedData = null;
        equipped = null;
    }
    
    private void OnDestroy() { _dropAction.performed -= OnDropPerformed; }
}