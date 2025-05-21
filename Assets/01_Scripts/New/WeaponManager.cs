using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] Transform weaponHolder;
    WeaponController equipped;

    public bool CanEquipWeapon => equipped == null;

    public void EquipFromPickup(WeaponPickup pickup)
    {
        if (!CanEquipWeapon) return;

        var go = new GameObject("WeaponController_" + pickup.Data.weaponName);
        go.transform.SetParent(weaponHolder, false);

        var wc  = go.AddComponent<WeaponController>();
        var ctx = GetComponent<WeaponContext>();
        wc.Initialize(pickup.Data, ctx);

        equipped = wc;
    }

    public void DropWeapon()
    {
        if (equipped == null) return;
        /*
        // Spawn a new world pickup
        var data = equipped.Data;
        var pickupGO = Instantiate(data.worldPickupPrefab);
        pickupGO.transform.position = weaponHolder.position;
        // Optionally give it a bit of forward force

        Destroy(equipped.gameObject);
        equipped = null;
        */
    }
}