using UnityEngine;

[CreateAssetMenu (menuName = "Consumables/ConsumableData")]
public class ConsumableDataSO : ScriptableObject
{
    public enum ConsumableType
    {
        Health,
        Stamina,
        Buff,
        Debuff
    }
    public ConsumableType type;
    public float amount = 5;
    public int charges = 1;
}