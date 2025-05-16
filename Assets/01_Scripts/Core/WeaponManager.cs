using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private WeaponDataSO weaponData;
    [SerializeField] private Transform weaponHolder;
    public InputActionReference dropWeaponAction;
    
    private GameObject _weaponInstance;
    
    public bool CanEquipWeapon => weaponHolder.childCount == 0;
    
    private void OnEnable()
    {
        if (dropWeaponAction != null)
        {
            dropWeaponAction.action.performed += OnDropWeapon;
            dropWeaponAction.action.Enable();
        }
    }
    
    public void EquipWeapon(WeaponDataSO weaponData, bool forceEquip = false)
    {
        if (weaponHolder.childCount > 0)
        {
            Debug.Log("Already have weapon equipped");
            return;
        }
        
        // Instantiate the weapon prefab
        GameObject weaponPrefab = weaponData.weaponPrefab;
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
        
        Debug.Log("Weapon equipped: " + weaponData.weaponName);
    }
    
    private void OnDropWeapon(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UnequipWeapon();
        }
    }
    
    private void UnequipWeapon()
    {
        if (weaponHolder.childCount == 0)
        {
            Debug.Log("No weapon to unequip");
            return;
        }
        
        _weaponInstance.transform.SetParent(null);
        
        // Optionally, you can add logic to drop the weapon in the world
        // For example, instantiate the weapon prefab at the player's position
        
        //drop at feet
        _weaponInstance.transform.position = transform.position + Vector3.down * 0.5f;
        _weaponInstance.GetComponent<WeaponController>().UnequipWeapon();

        // Optionally, you can add logic to drop the weapon in the world
        // For example, instantiate the weapon prefab at the player's position
        
        
    }
}
