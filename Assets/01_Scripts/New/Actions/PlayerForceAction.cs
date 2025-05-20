using UnityEngine;

[System.Serializable]
public class PlayerForceAction : WeaponActionData
{
    public float forceAmount = 10f;
    //direction
    public Vector3 forceDirection = Vector3.back;
    //public float forceDuration = 1f;
    //public AnimationCurve forceCurve;

    public override void Execute(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        if (ctx == null || ctx.Player == null) return;

        // Apply force to the player take player rotation into account
        Vector3 force = ctx.Player.transform.TransformDirection(forceDirection) * forceAmount;
        ctx.Player.ApplyForce(force);
    }
}