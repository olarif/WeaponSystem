using UnityEngine;

[System.Serializable]
public class ChargeDamageAction : WeaponActionData
{
    public float maxDistance, damageAmount;
    public DamageType damageType;

    public override void OnPress(WeaponContext ctx, WeaponDataSO.InputBinding b)
    {
        var origin = ctx.FirePoints[0].position;
        var dir    = ctx.FirePoints[0].forward;
        if (Physics.Raycast(origin, dir, out var hit, maxDistance) &&
            hit.collider.TryGetComponent<IDamageable>(out var d))
            d.TakeDamage(damageAmount, damageType);
    }
}