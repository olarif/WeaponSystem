using UnityEngine;

[System.Serializable]
public class PlayerForceAction : IWeaponAction
{
    public float forceAmount = 10f;
    public Vector3 forceDirection = Vector3.back;

    public void Execute(WeaponContext ctx, InputBindingData binding, ActionBindingData actionBinding)
    {
        if (ctx.Player == null) return;
        Vector3 force = ctx.Player.transform.TransformDirection(forceDirection) * forceAmount;
        ctx.Player.ApplyForce(force);
    }
}