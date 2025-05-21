using UnityEngine;

public class WeaponPickup : BaseInteractable
{
    [Header("Weapon Settings")]
    [SerializeField] private WeaponDataSO weaponData;

    public override void Interact(WeaponManager weaponManager)
    {
        if (weaponManager.CanEquipWeapon)
        {
            weaponManager.EquipFromPickup(this);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Cannot equip weapon, already have one equipped");
        }
    }

    public WeaponDataSO Data => weaponData;
}