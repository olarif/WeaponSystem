using UnityEngine;

/// <summary>
/// Destroys the weapon when used.
/// </summary>
[System.Serializable]
public class DestroyOnUseAction : IWeaponAction
{
    public void Execute(WeaponContext ctx, InputBindingData binding, ActionBindingData actionBinding)
    {
        if (ctx == null || ctx.WeaponManager == null) return;

        Destroy(ctx);
    }
    
    private void Destroy(WeaponContext ctx)
    {
        if (ctx.WeaponManager != null)
            ctx.WeaponManager.DestroyCurrentWeapon();
    }
}