using UnityEngine;

[System.Serializable]
public abstract class WeaponActionData
{
    public abstract void Execute(WeaponContext context, WeaponDataSO.InputBinding binding);
}