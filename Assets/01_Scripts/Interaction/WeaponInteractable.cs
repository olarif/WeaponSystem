using UnityEngine;

public class WeaponInteractable : BaseInteractable
{
    [Header("Weapon Settings")]
    private WeaponDataSO _weaponData;
    private WeaponController _weaponController;

    protected override void Awake()
    {
        base.Awake();
        _weaponController = GetComponent<WeaponController>();
        _weaponData = _weaponController.weaponData;
    }

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