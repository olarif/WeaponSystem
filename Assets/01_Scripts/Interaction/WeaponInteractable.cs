using UnityEngine;

public class WeaponInteractable : BaseInteractable
{
    [Header("Weapon Settings")]
    [SerializeField] private WeaponDataSO _weaponData;
    private GameObject _weaponPrefab;

    public override void Interact(WeaponManager weaponManager)
    {
        if (weaponManager.CanEquipWeapon)
        {
            weaponManager.EquipWeapon(_weaponData);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Cannot equip weapon, already have one equipped");
        }
    }
}