
public interface IWeaponAction
{
    void Execute(WeaponContext ctx,
        InputBindingData binding,
        ActionBindingData actionBinding);
}