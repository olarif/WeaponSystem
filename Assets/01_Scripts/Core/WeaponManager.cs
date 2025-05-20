using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder;
    public InputActionReference dropWeaponAction;
    
    private GameObject _weaponInstance;
    public GameObject WeaponInstance => _weaponInstance;
    
    public bool CanEquipWeapon => weaponHolder.childCount == 0;
    
    private void OnEnable()
    {
        if (dropWeaponAction != null)
        {
            dropWeaponAction.action.performed += OnDropWeapon;
            dropWeaponAction.action.Enable();
        }
    }
    
    public void EquipWeapon(WeaponDataSO data, bool forceEquip = false)
    {
        if (weaponHolder.childCount > 0)
        {
            Debug.Log("Already have weapon equipped");
            return;
        }
        /*
        // Instantiate the weapon prefab
        GameObject weaponPrefab = data.weaponPrefab;
        if (weaponPrefab == null)
        {
            Debug.LogError("Weapon prefab is null");
            return;
        }
        
        _weaponInstance = Instantiate(weaponPrefab, weaponHolder.position, weaponHolder.rotation);
        _weaponInstance.transform.SetParent(weaponHolder);
        _weaponInstance.transform.localPosition = Vector3.zero;
        _weaponInstance.transform.localRotation = Quaternion.identity;
        _weaponInstance.transform.localScale = Vector3.one;
        
        _weaponInstance.GetComponent<WeaponController>().EquipWeapon();
        */
        
        Debug.Log("Weapon equipped: " + data.weaponName);
    }
    
    private void OnDropWeapon(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UnequipWeapon();
        }
    }
    
    public void UnequipWeapon()
    {
        if (weaponHolder.childCount == 0) { return; }
        if (_weaponInstance == null) { return; }
        
        // Detach the weapon from the holder
        _weaponInstance.transform.SetParent(null);
        
        // Drop the weapon in the world
        RaycastHit hit;
        Physics.Raycast(transform.position, -Vector3.up, out hit, 10f);
        if (hit.collider != null)
        {
            _weaponInstance.transform.position = hit.point + Vector3.up * 0.05f; // Adjust height to avoid clipping
            _weaponInstance.transform.rotation = Quaternion.Euler(90, 0, 0);
        }

        //_weaponInstance.GetComponent<WeaponController>().UnEquipWeapon();
    }
}
