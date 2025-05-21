using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class DestroyOnUseAction : WeaponActionData
{
    public override void Execute(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        // Destroy the model
        ctx.WeaponManager.DestroyWeapon();
    }
}