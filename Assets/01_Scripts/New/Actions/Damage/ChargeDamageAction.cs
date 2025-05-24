using UnityEngine;

[System.Serializable]
public class ChargeDamageAction : IWeaponAction
{
    public float maxDistance, damageAmount;
    public DamageType damageType;

    public void Execute(WeaponContext ctx, InputBindingData binding, ActionBindingData actionBinding)
    {
        var origin = ctx.FirePoints[0].position;
        var dir    = ctx.FirePoints[0].forward;
        if (Physics.Raycast(origin, dir, out var hit, maxDistance) &&
            hit.collider.TryGetComponent<IDamageable>(out var d))
            d.TakeDamage(damageAmount, damageType);
    }
}