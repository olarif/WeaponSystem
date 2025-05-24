using UnityEngine;

[System.Serializable]
public class PlayerForceAction : WeaponActionData
{
    public float forceAmount = 10f;
    public Vector3 forceDirection = Vector3.back;

    public override void OnPress(WeaponContext ctx, WeaponDataSO.InputBinding b)
    {
        if (ctx.Player == null) return;
        Vector3 force = ctx.Player.transform.TransformDirection(forceDirection) * forceAmount;
        ctx.Player.ApplyForce(force);
    }
}