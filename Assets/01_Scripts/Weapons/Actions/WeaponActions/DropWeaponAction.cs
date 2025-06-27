using UnityEngine;

/// <summary>
/// Drops the currently equipped weapon when executed.
/// </summary>
[System.Serializable]
public class DropWeaponAction : IWeaponAction
{
    public void Execute(WeaponContext ctx, InputBindingData binding, ActionBindingData actionBinding)
    {
        if (ctx == null || ctx.WeaponManager == null) return;
        
        DropWeapon(ctx);
    }

    private void DropWeapon(WeaponContext ctx)
    {
        if (ctx.WeaponManager != null)
            ctx.WeaponManager.DropWeapon();
    }
}