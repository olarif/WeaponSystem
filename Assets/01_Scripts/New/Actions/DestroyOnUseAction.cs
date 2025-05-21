using UnityEngine;

[System.Serializable]
public class DestroyOnUseAction : WeaponActionData
{
    public override void Execute(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        // Destroy the model
        if (ctx.WeaponController?._model != null)
            Object.Destroy(ctx.WeaponController._model);
        
        // Destroy the WeaponController itself
        if (ctx.WeaponController != null)
            Object.Destroy(ctx.WeaponController.gameObject);
    }
}