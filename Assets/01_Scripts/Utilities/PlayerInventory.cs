using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private int maxWeapons = 2;
    
    [Header("Inventory Settings")]
    [SerializeField] private int maxInventorySlots = 20;
    
    private List<ItemData> inventory = new List<ItemData>();
    private List<int> itemQuantities = new List<int>();
    
    public WeaponManager WeaponManager => weaponManager;
    
    public bool CanEquipWeapon(WeaponDataSO weaponData)
    {
        return weaponManager != null && weaponManager.CanEquipWeapon;
    }
    
    public bool TryEquipWeapon(WeaponDataSO weaponData)
    {
        if (weaponManager != null && weaponManager.CanEquipWeapon)
        {
            // Your existing weapon equip logic
            weaponManager.TryEquipWeapon(weaponData);
            return true;
        }
        return false;
    }
    
    public bool CanAddItem(ItemData itemData, int quantity)
    {
        // Check if item can stack with existing items or if there's space
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].itemName == itemData.itemName && 
                itemQuantities[i] + quantity <= itemData.maxStackSize)
            {
                return true;
            }
        }
        
        return inventory.Count < maxInventorySlots;
    }
    
    public bool TryAddItem(ItemData itemData, int quantity)
    {
        // Try to stack with existing item
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].itemName == itemData.itemName && 
                itemQuantities[i] + quantity <= itemData.maxStackSize)
            {
                itemQuantities[i] += quantity;
                return true;
            }
        }
        
        // Add as new item if there's space
        if (inventory.Count < maxInventorySlots)
        {
            inventory.Add(itemData);
            itemQuantities.Add(quantity);
            return true;
        }
        
        return false;
    }
}