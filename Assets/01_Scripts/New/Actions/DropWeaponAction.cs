using UnityEngine;

[System.Serializable]
public class DropWeaponAction : WeaponActionData
{
    public override void Execute(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        if (ctx.WeaponManager != null)
            ctx.WeaponManager.DropWeapon();
    }
}