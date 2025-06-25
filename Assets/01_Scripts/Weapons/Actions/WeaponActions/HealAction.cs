using UnityEngine;

[System.Serializable]
public class HealAction : IWeaponAction
{
    public float amount = 10f;
    
    public void Execute(WeaponContext ctx, InputBindingData binding, ActionBindingData actionBinding)
    {
        if (ctx == null || ctx.Player == null) return;

        // Heal the player
        ctx.Player.GetComponentInChildren<HealthComponent>().Heal(amount);
        
        Debug.Log($"Healed {ctx.Player.name} for {amount} health.");
    }
}