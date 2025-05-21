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
        
        Vector3 dropPos;
        var cam = ctx.PlayerCamera;
        var ori = cam.transform.position;
        var dir = cam.transform.forward;
        const float maxDist = 2f;      // how far in front of camera
        const float downDist = 5f;     // how far to search downward
        
        if (Physics.Raycast(ori, dir, out var hitFwd, maxDist))
        {
            // if we hit a wall/ground, use that point
            dropPos = hitFwd.point;
        }
        else if (Physics.Raycast(ori + dir * maxDist, Vector3.down, out var hitDown, downDist))
        {
            // else drop straight down at the end of forward ray
            dropPos = hitDown.point;
        }
        else
        {
            // fallback to a fixed offset off the player’s feet
            dropPos = ctx.transform.position + dir * 1f;
        }
        
        // 2) Spawn the world‐pickup
        
        //rotate to be flat on floor 90 degrees
        var rot = Quaternion.Euler(90, 0, 0);
        
        var worldPickup = Instantiate(worldPickupPrefab, dropPos, rot);
        if (worldPickup.TryGetComponent<WeaponPickup>(out var wp))
            wp.Initialize(equippedData);

        // 3) Destroy every attached model before killing controller
        foreach (var mdl in ctx.WeaponController._models)
        {
            if (mdl != null) Destroy(mdl);
        }
        ctx.WeaponController._models.Clear();

        // Finally destroy the controller GameObject
        Destroy(ctx.WeaponController.gameObject);

        equipped     = null;
        equippedData = null;
        ctx.WeaponController = null;
    }

    public void DestroyWeapon()
    {
        // 1) Destroy every attached model before killing controller
        foreach (var mdl in ctx.WeaponController._models)
        {
            if (mdl != null) Destroy(mdl);
        }
        ctx.WeaponController._models.Clear();

        // Finally destroy the controller GameObject
        Destroy(ctx.WeaponController.gameObject);

        equipped     = null;
        equippedData = null;
        ctx.WeaponController = null;
    }
    
    private void OnDestroy() { _dropAction.performed -= OnDropPerformed; }
}