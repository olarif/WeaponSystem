using UnityEngine;

[System.Serializable]
public class HealAction : WeaponActionData
{
    public float amount = 10f;
    
    public override void OnPress(WeaponContext ctx, WeaponDataSO.InputBinding b)
    {
        if (ctx == null || ctx.Player == null) return;

        // Heal the player
        ctx.Player.GetComponentInChildren<HealthComponent>().Heal(amount);
        
        Debug.Log($"Healed {ctx.Player.name} for {amount} health.");
    }
}