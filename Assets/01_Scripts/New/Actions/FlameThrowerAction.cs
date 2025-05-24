using UnityEngine;

[System.Serializable]
public class FlameThrowerAction
{
    /*public float range           = 5f;
    public float halfAngle       = 30f;            // degrees
    public float damagePerSecond = 10f;
    public LayerMask targetMask;
    
    public override void OnContinuous(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        Execute(ctx, binding);
    }

    private void Execute(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        Transform fp = ctx.FirePoints.Count>0 
            ? ctx.FirePoints[0] 
            : ctx.rightHand;
        Vector3 origin = fp.position;
        Vector3 forward= fp.forward;
        float dmg = damagePerSecond * Time.deltaTime;

        foreach (var col in Physics.OverlapSphere(origin, range, targetMask))
        {
            Vector3 to = (col.transform.position - origin).normalized;
            if (Vector3.Angle(forward, to) <= halfAngle
                && col.TryGetComponent<IDamageable>(out var d))
                d.TakeDamage(dmg, DamageType.Fire);
        }
    }*/
}