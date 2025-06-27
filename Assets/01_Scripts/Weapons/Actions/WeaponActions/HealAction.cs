using UnityEngine;

/// <summary>
/// Heals the player when executed
/// </summary>
[System.Serializable]
public class HealAction : IWeaponAction
{
    public float amountToHeal = 10f;
    
    public void Execute(WeaponContext ctx, InputBindingData binding, ActionBindingData actionBinding)
    {
        if (ctx == null || ctx.Player == null) return;

        // Heal the player
        ctx.Player.GetComponentInChildren<HealthComponent>().Heal(amountToHeal);
        
        Debug.Log($"Healed {ctx.Player.name} for {amountToHeal} health.");
    }
}