using UnityEngine;

[System.Serializable]
public class FlameThrowerAction : WeaponActionData
{
    public float range           = 5f;
    public float halfAngle       = 30f;            // degrees
    public float damagePerSecond = 10f;
    public LayerMask targetMask;

    public override void Execute(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        // 1) origin & forward from your fire-point
        Transform fp = ctx.FirePoints.Count>0 ? ctx.FirePoints[0] : ctx.rightHand;
        Vector3 origin  = fp.position;
        Vector3 forward = fp.forward;

        // 2) find everything in a sphere
        var hits = Physics.OverlapSphere(origin, range, targetMask);
        float dmgThisTick = damagePerSecond * Time.deltaTime;

        // 3) filter by cone angle
        foreach (var col in hits)
        {
             //debug draw
            Debug.DrawLine(origin, col.transform.position, Color.red);
            Debug.DrawLine(origin, origin + forward * range, Color.green);

            Vector3 toTarget = (col.transform.position - origin).normalized;
            if (Vector3.Angle(forward, toTarget) <= halfAngle)
            {
                if (col.TryGetComponent<IDamageable>(out var d))
                    d.TakeDamage(dmgThisTick, DamageType.Fire);
            }
        }
    }
}