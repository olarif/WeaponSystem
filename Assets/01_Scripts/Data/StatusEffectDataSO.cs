using UnityEngine;

[CreateAssetMenu(menuName = "Projectiles/StatusEffectData")]
public class StatusEffectDataSO : ScriptableObject
{
    public enum StatusEffectType
    {
        Slow,
        Stun,
        Burn,
        Freeze,
        Poison,
        Bleed,
        Shock,
    }
    
    public StatusEffectType statusEffectType;
    public float duration;
    public float tickRate;
    public float tickDamage;
    public float impactDamage;

}