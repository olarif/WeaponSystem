using System;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private WeaponDataSO weaponData;
    [SerializeField] private Transform weaponHolder;
    
    public bool CanEquipWeapon => weaponHolder.childCount == 0;
    
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
        GameObject weaponInstance = Instantiate(weaponPrefab, weaponHolder.position, weaponHolder.rotation);
        weaponInstance.transform.SetParent(weaponHolder);
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.transform.localRotation = Quaternion.identity;
        weaponInstance.transform.localScale = Vector3.one;
        
        Debug.Log("Weapon equipped: " + weaponData.weaponName);
    }
    
    public void UnequipWeapon()
    {
        if (weaponHolder.childCount == 0)
        {
            Debug.Log("No weapon to unequip");
            return;
        }

        // Destroy the current weapon
        Destroy(weaponHolder.GetChild(0).gameObject);
        Debug.Log("Weapon unequipped");
    }
}
