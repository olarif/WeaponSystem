using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class DealMeleeDamageAction : WeaponActionData
{
    public DamageType damageType;
    public float damage = 5f;
    public float range = 2.5f;

    public override void OnPress(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        Collider[] hits = Physics.OverlapSphere(ctx.transform.position, range);
        foreach (var c in hits)
        {
            if (c.TryGetComponent<IDamageable>(out var d))
                d.TakeDamage(damage, damageType);
        }
    }
}