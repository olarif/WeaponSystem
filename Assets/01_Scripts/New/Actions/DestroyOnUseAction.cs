using UnityEngine;

[System.Serializable]
public class DestroyOnUseAction : WeaponActionData
{
    public override void OnPress(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        Destroy(ctx);
    }
    
    public override void OnRelease(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        Destroy(ctx);
    }
    
    private void Destroy(WeaponContext ctx)
    {
        if (ctx.WeaponManager != null)
            ctx.WeaponManager.DestroyWeapon();
    }
}