using UnityEngine;

public class UseConsumableExecute : ExecuteComponent
{
    public ConsumableDataSO consumableData;

    private Entity _player;
    private int _charges;
    private bool _hasInitialized;

    public override void Initialize(WeaponContext context)
    {
        base.Initialize(context);
        _player = context.Player;
        
        if(!_hasInitialized)
        {
            _charges = consumableData != null ? consumableData.charges : 0;
            _hasInitialized = true;
        }
    }
    
    public override void OnStart()
    {
        
    }

    public override void OnStop()
    {
        
    }

    public override void Execute()
    {
        if (_charges <= 0)
        {
            _player.GetComponent<WeaponManager>().UnequipWeapon();
            return;
        }
        
        switch (consumableData.type)
        {
            case ConsumableDataSO.ConsumableType.Health:
                _player.Heal(consumableData.amount);
                break;
            case ConsumableDataSO.ConsumableType.Stamina:
                _player.RestoreStamina(consumableData.amount);
                break;
        }
        
        // Consume one charge
        _charges--;
        Debug.Log($"[{name}] consumed, remaining: {_charges}");

        // If that was the last one, auto‐unequip
        if (_charges <= 0)
            _player.GetComponent<WeaponManager>().UnequipWeapon();
    }
}