using UnityEngine;

/// <summary>
/// Pickup object that equips a weapon when interacted with.
/// </summary>
public class WeaponPickup : BasePickup
{
    [Header("Weapon Data")]
    [SerializeField] private WeaponDataSO weaponData;

    public override string GetPromptText()
    {
        
        //return weaponData != null ? $"E to pickup {weaponData.weaponName}" : "E to pickup weapon";
        //return "E to pickup";
        return promptText;
    }
    
    public override bool CanPickup(PlayerInventory inventory)
    {
        return weaponData != null && inventory.CanEquipWeapon(weaponData);
    }
    
    public override void OnPickup(PlayerInventory inventory)
    {
        if (weaponData != null)
        {
            inventory.TryEquipWeapon(weaponData);
            Destroy(gameObject);
        }
    }

    public void Initialize(WeaponDataSO data)
    {
        weaponData = data;
    }

    private void PlayPickupEffect()
    {
        
    }

    public WeaponDataSO Data => weaponData;
}