using UnityEngine;

[System.Serializable]
public class HealAction : WeaponActionData
{
    public float amount = 10f;

    public override void Execute(WeaponContext ctx, WeaponDataSO.InputBinding binding)
    {
        if (ctx == null || ctx.Player == null) return;
        
        ctx.Player.GetComponentInChildren<HealthComponent>().Heal(amount);
        
        Debug.Log($"Healed {ctx.Player.name} for {amount} health.");
    }
}