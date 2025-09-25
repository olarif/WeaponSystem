using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;

public class WeaponManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private DisplayWeaponStats displayWeaponStats;
    [SerializeField] private InputActionReference dropWeaponAction;
    
    [Header("Drop Settings")]
    [SerializeField] private float dropDistance = 2f;
    [SerializeField] private float fallbackDistance = 1f;
    
    private WeaponController equippedWeapon;
    private WeaponDataSO equippedData;
    private WeaponContext weaponContext;
    private NetworkWeaponManager networkManager;
    
    public bool CanEquipWeapon => equippedWeapon == null;
    public bool HasWeaponEquipped => equippedWeapon != null;
    public WeaponDataSO EquippedWeaponData => equippedData;
    
    private void Awake()
    {
        weaponContext = GetComponent<WeaponContext>();
        if (weaponContext != null)
            weaponContext.WeaponManager = this;
        
        networkManager = GetComponent<NetworkWeaponManager>();
    }
    
    private void OnEnable()
    {
        if (!IsOwner) return;
        
        if (dropWeaponAction?.action != null)
        {
            dropWeaponAction.action.performed += _ => TryDropWeapon();
            dropWeaponAction.action.Enable();
        }
    }
    
    private void OnDisable()
    {
        if (!IsOwner) return;
        
        if (dropWeaponAction?.action != null)
        {
            dropWeaponAction.action.performed -= _ => TryDropWeapon();
            dropWeaponAction.action.Disable();
        }
    }
    
    public bool TryEquipWeapon(WeaponDataSO weaponData)
    {
        if (!IsOwner) return false;
        if (!CanEquipWeapon || weaponData == null) return false;
        
        var weaponObject = new GameObject($"WeaponController_{weaponData.weaponName}");
        weaponObject.transform.SetParent(weaponHolder, false);
        
        var weaponController = weaponObject.AddComponent<WeaponController>();
        weaponController.Initialize(weaponData, weaponContext);
        
        equippedWeapon = weaponController;
        equippedData = weaponData;
        weaponContext.WeaponController = weaponController;
        networkManager?.OnWeaponPickedUp(weaponData);
        
        displayWeaponStats?.DisplayStats(weaponData.weaponName, weaponData.weaponDescription);
        return true;
    }
    
    public bool EquipFromPickup(WeaponPickup pickup)
    {
        return pickup?.Data != null && TryEquipWeapon(pickup.Data);
    }
    
    public bool TryDropWeapon()
    {
        if (!IsOwner) return false;
        if (!HasWeaponEquipped) return false;
        
        Vector3 dropPos = CalculateDropPosition();
        var droppedWeapon = Instantiate(equippedData.pickupPrefab, dropPos, Quaternion.Euler(90, 0, 0));
        
        if (droppedWeapon.TryGetComponent<WeaponPickup>(out var pickup))
            pickup.Initialize(equippedData);
        
        DestroyCurrentWeapon();
        
        networkManager?.OnWeaponDropped();
        
        return true;
    }
    
    public void DestroyCurrentWeapon()
    {
        if (!HasWeaponEquipped) return;
        
        // Clean up models
        if (weaponContext?.WeaponController?._models != null)
        {
            foreach (var model in weaponContext.WeaponController._models)
                if (model != null) Destroy(model);
            weaponContext.WeaponController._models.Clear();
        }
        
        // Clean up controller
        if (equippedWeapon?.gameObject != null)
            Destroy(equippedWeapon.gameObject);
        
        // Clear references
        equippedWeapon = null;
        equippedData = null;
        if (weaponContext != null)
            weaponContext.WeaponController = null;
        
        displayWeaponStats?.ClearStats();
    }
    
    public bool ReplaceWeapon(WeaponDataSO newWeapon, bool dropCurrent = true)
    {
        if (HasWeaponEquipped)
        {
            if (dropCurrent) TryDropWeapon();
            else DestroyCurrentWeapon();
        }
        return TryEquipWeapon(newWeapon);
    }
    
    private Vector3 CalculateDropPosition()
    {
        if (weaponContext?.PlayerCamera == null)
            return transform.position + transform.forward * fallbackDistance;
        
        var camera = weaponContext.PlayerCamera;
        var origin = camera.transform.position;
        var forward = camera.transform.forward;
        
        // Try dropping in front of player
        if (Physics.Raycast(origin, forward, out var hit, dropDistance))
            return hit.point;
        
        // Try dropping down from forward position
        var endPoint = origin + forward * dropDistance;
        if (Physics.Raycast(endPoint, Vector3.down, out hit, 5f))
            return hit.point;
        
        // Fallback
        return transform.position + forward * fallbackDistance;
    }
}