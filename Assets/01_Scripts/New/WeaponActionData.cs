[System.Serializable]
public abstract class WeaponActionData
{
    public virtual void OnPress(WeaponContext ctx, WeaponDataSO.InputBinding b) {}
    public virtual void OnRelease(WeaponContext ctx, WeaponDataSO.InputBinding b) {}
    public virtual void OnContinuous(WeaponContext ctx, WeaponDataSO.InputBinding b) {}
}