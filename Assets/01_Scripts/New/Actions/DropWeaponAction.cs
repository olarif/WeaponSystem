using UnityEngine;

[System.Serializable]
public class DropWeaponAction : WeaponActionData
{
    public override void OnPress(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        DropWeapon(ctx);
    }
    
    public override void OnRelease(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        DropWeapon(ctx);
    }

    private void DropWeapon(WeaponContext ctx)
    {
        if (ctx.WeaponManager != null)
            ctx.WeaponManager.DropWeapon();
    }
}